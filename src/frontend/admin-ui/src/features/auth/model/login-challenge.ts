import type { LoginChallenge } from './auth.schemas'

export const loginChallengeRestartNotice = 'Your MFA challenge expired or was rejected. Enter your credentials to start again.'

export function isLoginChallengeExpired(challenge: LoginChallenge, nowUtcMs = Date.now()) {
    return Date.parse(challenge.expiresAtUtc) <= nowUtcMs
}
