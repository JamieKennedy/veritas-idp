import { toast } from 'sonner'

import { getOperationalToast } from './request-errors'

export function notifyOperationalRequestError(error: unknown) {
    if (typeof window === 'undefined') {
        return
    }

    const notification = getOperationalToast(error)
    if (notification === undefined) {
        return
    }

    toast.error(notification.title, {
        id: notification.id,
        description: notification.description,
    })
}
