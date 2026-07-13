/* eslint-disable react-refresh/only-export-components -- TanStack Router route modules co-locate route definitions. */
import { createFileRoute } from '@tanstack/react-router'

import { sanitizeLoginRedirect } from '@/app/routing/destinations'
import { LoginPage } from '@/features/auth/pages/login-page'

export const Route = createFileRoute('/_guest/login')({
    validateSearch: (search: Record<string, unknown>) => ({ redirect: sanitizeLoginRedirect(search.redirect) }),
    component: LoginRoute,
})

function LoginRoute() {
    const { redirect } = Route.useSearch()
    const { setup } = Route.useRouteContext()
    return <LoginPage redirectTo={redirect} setup={setup} />
}
