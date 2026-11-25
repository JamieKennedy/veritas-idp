import { useState } from 'react'
import { Check, Clipboard, Terminal } from 'lucide-react'

import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'

interface InstallPreviewProps {
    commands: Record<Shell, Array<string>>
    description: string
}

type Shell = 'bash' | 'powershell'

const shells: Array<{ label: string; prompt: string; value: Shell }> = [
    { label: 'Bash', prompt: '$', value: 'bash' },
    { label: 'PowerShell', prompt: 'PS>', value: 'powershell' },
]

export function InstallPreview({ commands, description }: InstallPreviewProps) {
    const [copied, setCopied] = useState(false)
    const [shell, setShell] = useState<Shell>('bash')
    const source = commands[shell].join('\n')

    async function copy() {
        await navigator.clipboard.writeText(source)
        setCopied(true)
        window.setTimeout(() => {
            setCopied(false)
        }, 1800)
    }

    return (
        <Card className="terminal-card">
            <CardHeader>
                <div className="flex items-start gap-3">
                    <span className="terminal-icon">
                        <Terminal className="size-4" />
                    </span>
                    <div>
                        <CardTitle>From release bundle to running stack</CardTitle>
                        <CardDescription>{description}</CardDescription>
                    </div>
                </div>
            </CardHeader>
            <CardContent>
                <Tabs
                    value={shell}
                    onValueChange={(value) => {
                        setShell(value as Shell)
                        setCopied(false)
                    }}
                    className="terminal-window"
                >
                    <div className="terminal-bar">
                        <span className="terminal-dots" aria-hidden="true">
                            <i />
                            <i />
                            <i />
                        </span>
                        <TabsList className="terminal-tabs" aria-label="Installation shell">
                            {shells.map((item) => (
                                <TabsTrigger key={item.value} value={item.value}>
                                    {item.label}
                                </TabsTrigger>
                            ))}
                        </TabsList>
                        <Button
                            type="button"
                            variant="ghost"
                            size="sm"
                            onClick={() => {
                                void copy()
                            }}
                            aria-label={`Copy ${shell === 'bash' ? 'Bash' : 'PowerShell'} installation commands`}
                        >
                            {copied ? <Check /> : <Clipboard />}
                            {copied ? 'Copied' : 'Copy'}
                        </Button>
                    </div>
                    {shells.map((item) => (
                        <TabsContent key={item.value} value={item.value} className="terminal-content">
                            <pre aria-label={`${item.label} Docker Compose installation preview`}>
                                {commands[item.value].map((command, commandIndex) => (
                                    <code key={`${String(commandIndex)}-${command}`}>
                                        <span aria-hidden="true">{item.prompt}</span> {command}
                                        {'\n'}
                                    </code>
                                ))}
                            </pre>
                        </TabsContent>
                    ))}
                    <span role="status" aria-live="polite" className="sr-only">
                        {copied ? `${shell === 'bash' ? 'Bash' : 'PowerShell'} installation commands copied to the clipboard.` : ''}
                    </span>
                </Tabs>
            </CardContent>
        </Card>
    )
}
