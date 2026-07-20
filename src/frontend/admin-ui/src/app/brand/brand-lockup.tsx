import veritasLockupUrl from '@brand/veritas-lockup.svg?url'
import veritasMarkUrl from '@brand/veritas-mark.svg?url'

export function BrandLockup({ className = '' }: { className?: string }) {
    return (
        <div className={`brand-lockup ${className}`} role="img" aria-label="Veritas">
            <img src={veritasLockupUrl} alt="" aria-hidden="true" className="brand-lockup__light" />
            <div className="brand-lockup__dark" aria-hidden="true">
                <img src={veritasLockupUrl} alt="" className="brand-lockup__dark-wordmark" />
                <img src={veritasMarkUrl} alt="" className="brand-lockup__dark-mark" />
            </div>
        </div>
    )
}
