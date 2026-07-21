# Deployment secrets

Never commit `.env`, the `secrets` directory, bootstrap secrets, database passwords, RabbitMQ passwords, signing keys, or data-protection keys.

Generate independent high-entropy values. With OpenSSL:

```powershell
openssl rand -hex 32
openssl rand -hex 32
openssl rand -base64 48 | Set-Content -NoNewline secrets/bootstrap-secret.txt
```

Use the first two generated values for the Postgres and RabbitMQ password entries in `.env`. Hex keeps the RabbitMQ value safe when it is embedded in an AMQP URI. The third command writes the bootstrap secret file.

Protect the files so only the deployment account can read them. Back up the data-protection volume separately from configuration secrets: losing its keys can invalidate protected authentication state, while exposing them weakens that protection.

The bootstrap secret is accepted only by the narrow first-administrator setup flow. Rotate it after suspected exposure and remove unnecessary copies after bootstrap is complete according to the deployment's recovery policy.
