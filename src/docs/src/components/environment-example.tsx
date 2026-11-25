import { useState } from 'react'

import { Check, Clipboard, TriangleAlert } from 'lucide-react'

import { Button } from '@/components/ui/button'
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'

type CopyState = 'idle' | 'copied' | 'failed'

interface EnvironmentExampleProps {
    source: string
}

export function EnvironmentExample({ source }: EnvironmentExampleProps) {
    const [copyState, setCopyState] = useState<CopyState>('idle')

    async function copyEnvironmentExample() {
        try {
            await navigator.clipboard.writeText(source)
            setCopyState('copied')
        } catch {
            setCopyState('failed')
        }
    }

    const copyLabel = copyState === 'copied' ? 'Copied' : copyState === 'failed' ? 'Copy failed' : 'Copy'
    const CopyIcon = copyState === 'copied' ? Check : copyState === 'failed' ? TriangleAlert : Clipboard

    return (
        <Card className="environment-example border-border bg-card shadow-sm">
            <CardHeader>
                <CardTitle>Complete environment file</CardTitle>
                <CardDescription>This is rendered directly from the tracked .env.example file.</CardDescription>
                <CardAction>
                    <Button
                        type="button"
                        variant="outline"
                        onClick={() => {
                            void copyEnvironmentExample()
                        }}
                    >
                        <CopyIcon aria-hidden="true" data-icon="inline-start" />
                        {copyLabel}
                    </Button>
                </CardAction>
            </CardHeader>
            <CardContent>
                <pre
                    aria-label="Complete Veritas environment file example"
                    className="max-h-[32rem] overflow-auto rounded-lg border border-border bg-muted p-4 text-xs leading-relaxed text-foreground"
                >
                    <code>{source}</code>
                </pre>
                <span role="status" aria-live="polite" className="sr-only">
                    {copyState === 'copied'
                        ? 'Environment example copied to the clipboard.'
                        : copyState === 'failed'
                          ? 'The environment example could not be copied.'
                          : ''}
                </span>
            </CardContent>
        </Card>
    )
}
