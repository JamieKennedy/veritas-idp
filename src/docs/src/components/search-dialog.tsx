import { useEffect, useState } from 'react'
import { ArrowRight, FileText, Search } from 'lucide-react'

import { Button } from '@/components/ui/button'
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from '@/components/ui/command'
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog'

interface PagefindData {
    excerpt: string
    meta: {
        title?: string
    }
    url: string
}

interface PagefindResult {
    data: () => Promise<PagefindData>
}

interface PagefindModule {
    options: (options: { baseUrl: string }) => Promise<void>
    search: (query: string) => Promise<{ results: Array<PagefindResult> }>
}

interface SearchDialogProps {
    baseUrl: string
}

function plainText(value: string) {
    return value
        .replaceAll(/<[^>]+>/g, '')
        .replaceAll(/\s+/g, ' ')
        .trim()
}

export function SearchDialog({ baseUrl }: SearchDialogProps) {
    const [open, setOpen] = useState(false)
    const [query, setQuery] = useState('')
    const [results, setResults] = useState<Array<PagefindData>>([])
    const [searching, setSearching] = useState(false)
    const [unavailable, setUnavailable] = useState(false)

    useEffect(() => {
        const openSearch = (event: KeyboardEvent) => {
            if ((event.metaKey || event.ctrlKey) && event.key.toLowerCase() === 'k') {
                event.preventDefault()
                setOpen(true)
            }
        }
        window.addEventListener('keydown', openSearch)
        return () => {
            window.removeEventListener('keydown', openSearch)
        }
    }, [])

    useEffect(() => {
        if (!open || query.trim().length < 2) {
            return
        }

        let cancelled = false
        async function search() {
            setSearching(true)
            setUnavailable(false)
            try {
                const moduleUrl = `${baseUrl}/pagefind/pagefind.js`
                const pagefind = (await import(/* @vite-ignore */ moduleUrl)) as PagefindModule
                await pagefind.options({ baseUrl: `${baseUrl}/` })
                const response = await pagefind.search(query)
                const nextResults = await Promise.all(response.results.slice(0, 8).map(async (result) => result.data()))
                if (!cancelled) {
                    setResults(nextResults)
                }
            } catch {
                if (!cancelled) {
                    setResults([])
                    setUnavailable(true)
                }
            } finally {
                if (!cancelled) {
                    setSearching(false)
                }
            }
        }

        const timeout = window.setTimeout(() => {
            void search()
        }, 120)
        return () => {
            cancelled = true
            window.clearTimeout(timeout)
        }
    }, [baseUrl, open, query])

    function updateQuery(value: string) {
        setQuery(value)
        if (value.trim().length < 2) {
            setResults([])
            setSearching(false)
            setUnavailable(false)
        }
    }

    function updateOpen(value: boolean) {
        setOpen(value)
        if (!value) {
            setQuery('')
            setResults([])
            setSearching(false)
            setUnavailable(false)
        }
    }

    return (
        <Dialog open={open} onOpenChange={updateOpen}>
            <DialogTrigger asChild>
                <Button type="button" variant="outline" className="search-trigger">
                    <Search />
                    <span className="hidden sm:inline">Search docs</span>
                    <kbd className="hidden rounded border px-1.5 py-0.5 font-mono text-[0.65rem] text-muted-foreground sm:inline">⌘ K</kbd>
                </Button>
            </DialogTrigger>
            <DialogContent className="overflow-hidden p-0">
                <DialogHeader className="sr-only">
                    <DialogTitle>Search Veritas documentation</DialogTitle>
                    <DialogDescription>Search installation, operations, security, and development guides.</DialogDescription>
                </DialogHeader>
                <Command shouldFilter={false}>
                    <CommandInput value={query} onValueChange={updateQuery} placeholder="Search deployment, secrets, MFA…" autoFocus />
                    <CommandList>
                        {query.trim().length < 2 ? (
                            <div className="px-4 py-10 text-center text-sm text-muted-foreground">Enter at least two characters to search.</div>
                        ) : null}
                        {searching ? <div className="px-4 py-10 text-center text-sm text-muted-foreground">Searching documentation…</div> : null}
                        {unavailable ? (
                            <div className="px-4 py-10 text-center text-sm text-muted-foreground">
                                Search is available in the production documentation build.
                            </div>
                        ) : null}
                        {!searching && !unavailable && query.trim().length >= 2 && results.length === 0 ? (
                            <CommandEmpty>No documentation found.</CommandEmpty>
                        ) : null}
                        {results.length > 0 ? (
                            <CommandGroup heading="Documentation">
                                {results.map((result) => (
                                    <CommandItem
                                        key={result.url}
                                        value={result.url}
                                        onSelect={() => {
                                            window.location.assign(result.url)
                                        }}
                                    >
                                        <FileText className="mt-0.5 size-4 shrink-0 text-primary" />
                                        <span className="min-w-0 flex-1">
                                            <span className="block font-semibold">{result.meta.title ?? 'Veritas documentation'}</span>
                                            <span className="mt-0.5 line-clamp-2 block text-xs text-muted-foreground">{plainText(result.excerpt)}</span>
                                        </span>
                                        <ArrowRight className="mt-1 size-4 text-muted-foreground" />
                                    </CommandItem>
                                ))}
                            </CommandGroup>
                        ) : null}
                    </CommandList>
                </Command>
            </DialogContent>
        </Dialog>
    )
}
