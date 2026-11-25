---
title: Reverse proxy and HTTPS
description: Terminate trusted HTTPS in front of the loopback-bound Veritas Admin UI without exposing internal services.
section: Deploy and operate
sectionOrder: 1
order: 3
---

The supplied Compose topology publishes only the Admin UI, bound to `127.0.0.1:8080` by default. Admin API, PostgreSQL, Redis, and RabbitMQ remain on internal container networks. Keep that boundary intact.

## Required topology

Route all external traffic through one trusted HTTPS reverse proxy:

```text
Browser ──HTTPS──> reverse proxy ──HTTP on loopback──> Admin UI ──internal network──> Admin API
```

The Admin UI server proxies API requests to its internal `ADMIN_API_ORIGIN`. The external proxy should therefore forward the complete public origin to Admin UI rather than publishing Admin API separately.

## Requirements

- Use a hostname you control and a publicly trusted or organization-trusted certificate.
- Redirect plain HTTP to HTTPS at the edge.
- Forward the original host, scheme, and client-address headers.
- Apply sensible request-body, header, and idle timeouts.
- Keep the published Compose address on loopback unless the proxy runs on another explicitly trusted host.
- Do not publish PostgreSQL `5432`, RabbitMQ `5672` or `15672`, Redis `6379`, or the Admin API container port `8080` to untrusted networks.

Secure administrator and bootstrap cookies use the `__Host-` prefix and `Secure` policy. Browser flows will not behave correctly through an untrusted plain-HTTP origin.

## Caddy example

```text
admin.example.com {
    encode zstd gzip
    reverse_proxy 127.0.0.1:8080
}
```

Caddy can obtain and renew certificates automatically when DNS and network policy allow it. Review its storage and renewal behavior as part of the host backup and monitoring plan.

## NGINX example

```nginx
server {
    listen 80;
    server_name admin.example.com;
    return 308 https://$host$request_uri;
}

server {
    listen 443 ssl;
    server_name admin.example.com;

    ssl_certificate /etc/letsencrypt/live/admin.example.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/admin.example.com/privkey.pem;

    location / {
        proxy_pass http://127.0.0.1:8080;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-Host $host;
        proxy_set_header X-Forwarded-Proto https;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_read_timeout 60s;
    }
}
```

Replace the hostname and certificate paths. Keep proxy configuration, certificate automation, firewall rules, and private keys outside the Veritas release bundle.

## Verification

After starting Veritas:

1. Confirm the Admin UI is reachable through the HTTPS hostname.
2. Confirm direct access through a non-loopback host address is blocked.
3. Confirm HTTP redirects to the expected HTTPS origin.
4. Confirm bootstrap and authentication responses set `Secure` cookies.
5. Confirm the UI can call `/api/v1/setup/status` without CORS, mixed-content, or certificate errors.
6. Confirm Admin API and infrastructure ports are absent from external port scans.

If the proxy changes the public origin, update the allow-listed origin only through a reviewed deployment override. Never use a wildcard origin with credentialed administrator requests.
