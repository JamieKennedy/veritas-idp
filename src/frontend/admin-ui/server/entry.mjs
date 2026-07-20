import startApplication from '../dist/server/server.js'

import { createServerHandler, parseAdminApiOrigin } from './proxy.mjs'

const adminApiOrigin = parseAdminApiOrigin(process.env.ADMIN_API_ORIGIN)

export default createServerHandler({
    adminApiOrigin,
    application: startApplication,
})
