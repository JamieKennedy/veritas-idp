/* eslint-disable react-refresh/only-export-components -- TanStack Router route modules co-locate route definitions. */
import { HeadContent, Scripts, createRootRouteWithContext } from '@tanstack/react-router'

import { TanStackDevtools } from '@tanstack/react-devtools'
import type { QueryClient } from '@tanstack/react-query'
import { TanStackRouterDevtoolsPanel } from '@tanstack/react-router-devtools'
import TanStackQueryDevtools from '../integrations/tanstack-query/devtools'
import appCss from '../styles.css?url'
import { RouteError, RoutePending } from '@/app/routing/route-status'
import { Toaster } from '@/components/ui/sonner'

interface RouterContext {
    queryClient: QueryClient
}

export const Route = createRootRouteWithContext<RouterContext>()({
    errorComponent: RouteError,
    pendingComponent: RoutePending,
    head: () => ({
        meta: [
            {
                charSet: 'utf-8',
            },
            {
                name: 'viewport',
                content: 'width=device-width, initial-scale=1',
            },
            {
                title: 'Veritas Admin',
            },
        ],
        links: [
            {
                rel: 'stylesheet',
                href: appCss,
            },
        ],
    }),
    shellComponent: RootDocument,
})

function RootDocument({ children }: { children: React.ReactNode }) {
    const showDevtools = import.meta.env.DEV && import.meta.env.MODE !== 'test'

    return (
        <html lang="en">
            <head>
                <HeadContent />
            </head>
            <body>
                {children}
                <Toaster />
                {showDevtools && (
                    <TanStackDevtools
                        config={{
                            position: 'bottom-right',
                        }}
                        plugins={[
                            {
                                name: 'Tanstack Router',
                                render: <TanStackRouterDevtoolsPanel />,
                            },
                            TanStackQueryDevtools,
                        ]}
                    />
                )}
                <Scripts />
            </body>
        </html>
    )
}
