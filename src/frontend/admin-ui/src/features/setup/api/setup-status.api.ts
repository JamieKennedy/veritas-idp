import { apiRequest } from '@/lib/api/http-client'
import { setupStatusSchema } from '../model/setup-status.schema'

export function getSetupStatus(signal?: AbortSignal) {
    return apiRequest('/api/v1/setup/status', { schema: setupStatusSchema, signal })
}
