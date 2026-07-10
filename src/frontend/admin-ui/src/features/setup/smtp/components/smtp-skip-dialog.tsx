import { AlertTriangle } from 'lucide-react'
import { AlertDialog } from 'radix-ui'

import { Button } from '@/components/ui/button'

export function SmtpSkipDialog({ onSkip }: { onSkip: () => void }) {
    return (
        <AlertDialog.Root>
            <AlertDialog.Trigger asChild>
                <Button type="button" variant="outline">
                    Set up later
                </Button>
            </AlertDialog.Trigger>
            <AlertDialog.Portal>
                <AlertDialog.Overlay className="fixed inset-0 z-50 bg-black/45" />
                <AlertDialog.Content className="bg-background fixed top-1/2 left-1/2 z-50 w-[calc(100%-2rem)] max-w-lg -translate-x-1/2 -translate-y-1/2 rounded-lg border p-6 shadow-lg">
                    <div className="flex items-start gap-3">
                        <AlertTriangle className="mt-0.5 size-5 text-amber-600" />
                        <div>
                            <AlertDialog.Title className="text-lg font-semibold">Skip email setup?</AlertDialog.Title>
                            <AlertDialog.Description className="text-muted-foreground mt-2 text-sm">
                                Password recovery, notifications, invitations, and other email-dependent flows will remain unavailable until SMTP is configured.
                            </AlertDialog.Description>
                        </div>
                    </div>
                    <div className="mt-6 flex justify-end gap-3">
                        <AlertDialog.Cancel asChild>
                            <Button variant="outline">Return to setup</Button>
                        </AlertDialog.Cancel>
                        <AlertDialog.Action asChild>
                            <Button variant="destructive" onClick={onSkip}>
                                Skip email setup
                            </Button>
                        </AlertDialog.Action>
                    </div>
                </AlertDialog.Content>
            </AlertDialog.Portal>
        </AlertDialog.Root>
    )
}
