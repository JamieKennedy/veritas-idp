import { createFileRoute } from '@tanstack/react-router'

import { resolveRootRoute } from '@/app/routing/guards'

export const Route = createFileRoute('/')({
    ssr: false,
    beforeLoad: ({ context }) => resolveRootRoute(context.queryClient),
})
