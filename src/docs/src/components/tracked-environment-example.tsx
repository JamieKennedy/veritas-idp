import environmentExample from '../../../../.env.example?raw'

import { EnvironmentExample } from './environment-example'

export function TrackedEnvironmentExample() {
    return <EnvironmentExample source={environmentExample} />
}
