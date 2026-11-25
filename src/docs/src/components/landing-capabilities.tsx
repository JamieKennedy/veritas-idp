import { CheckCircle2, Construction, ShieldCheck } from 'lucide-react'

import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Badge } from '@/components/ui/badge'
import { Card, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import type { LandingCapability } from '@/content/landing'

interface LandingCapabilitiesProps {
    available: Array<LandingCapability>
    roadmap: Array<LandingCapability>
    warning: string
}

function CapabilityGrid({ capabilities, available }: { available: boolean; capabilities: Array<LandingCapability> }) {
    return (
        <div className="grid gap-4 md:grid-cols-2">
            {capabilities.map((capability) => (
                <Card key={capability.title} className="landing-card">
                    <CardHeader>
                        <div className="mb-3 flex items-center justify-between">
                            {available ? <CheckCircle2 className="size-5 text-primary" /> : <Construction className="size-5 text-muted-foreground" />}
                            <Badge variant={available ? 'default' : 'outline'}>{available ? 'Implemented' : 'Not yet available'}</Badge>
                        </div>
                        <CardTitle>{capability.title}</CardTitle>
                        <CardDescription>{capability.description}</CardDescription>
                    </CardHeader>
                </Card>
            ))}
        </div>
    )
}

export function LandingCapabilities({ available, roadmap, warning }: LandingCapabilitiesProps) {
    return (
        <div>
            <Alert variant="warning" className="mb-7">
                <ShieldCheck className="mt-0.5 size-4 text-warning" />
                <AlertTitle>Evaluate it; do not entrust it with production identities.</AlertTitle>
                <AlertDescription>{warning}</AlertDescription>
            </Alert>
            <Tabs defaultValue="available">
                <TabsList aria-label="Veritas capability maturity">
                    <TabsTrigger value="available">Available today</TabsTrigger>
                    <TabsTrigger value="roadmap">Still being built</TabsTrigger>
                </TabsList>
                <TabsContent value="available">
                    <CapabilityGrid capabilities={available} available />
                </TabsContent>
                <TabsContent value="roadmap">
                    <CapabilityGrid capabilities={roadmap} available={false} />
                </TabsContent>
            </Tabs>
        </div>
    )
}
