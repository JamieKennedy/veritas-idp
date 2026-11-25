import { useEffect } from 'react'

import { buttonVariants } from '@/components/ui/button'
import { cn } from '@/lib/utils'

const shellLabels: Record<string, string> = {
    bash: 'Bash',
    console: 'Console',
    powershell: 'PowerShell',
    ps1: 'PowerShell',
    sh: 'Bash',
    shell: 'Bash',
}

function languageLabel(pre: HTMLPreElement) {
    const language = pre.dataset.language?.toLowerCase() ?? 'text'
    return shellLabels[language] ?? language.toUpperCase()
}

function isShellBlock(element: Element | null): element is HTMLPreElement {
    return element instanceof HTMLPreElement && (element.dataset.language?.toLowerCase() ?? '') in shellLabels
}

function codeText(pre: HTMLPreElement) {
    return pre.querySelector('code')?.textContent ?? pre.textContent
}

function createCopyButton(activePre: () => HTMLPreElement) {
    const button = document.createElement('button')
    const announcement = document.createElement('span')
    let resetTimer: ReturnType<typeof setTimeout> | undefined

    button.type = 'button'
    button.className = cn(buttonVariants({ variant: 'ghost', size: 'sm' }), 'code-toolbar__copy')
    button.textContent = 'Copy'
    button.setAttribute('aria-label', 'Copy code')

    announcement.className = 'sr-only'
    announcement.setAttribute('aria-live', 'polite')

    const copy = async () => {
        try {
            await navigator.clipboard.writeText(codeText(activePre()))
            button.textContent = 'Copied'
            announcement.textContent = 'Code copied to clipboard'
        } catch {
            button.textContent = 'Copy failed'
            announcement.textContent = 'Code could not be copied'
        }

        clearTimeout(resetTimer)
        resetTimer = setTimeout(() => {
            button.textContent = 'Copy'
            announcement.textContent = ''
        }, 1800)
    }

    button.addEventListener('click', () => {
        void copy()
    })

    return { announcement, button }
}

function enhanceStandalone(pre: HTMLPreElement) {
    if (pre.dataset.codeEnhanced === 'true') {
        return
    }

    const wrapper = document.createElement('div')
    const toolbar = document.createElement('div')
    const label = document.createElement('span')

    wrapper.className = 'code-block'
    toolbar.className = 'code-toolbar'
    label.className = 'code-language'
    label.textContent = languageLabel(pre)

    pre.before(wrapper)
    wrapper.append(toolbar, pre)
    pre.dataset.codeEnhanced = 'true'

    const { announcement, button } = createCopyButton(() => pre)
    toolbar.append(label, button, announcement)
}

function enhanceShellGroup(marker: HTMLElement, groupIndex: number) {
    const panes: Array<HTMLPreElement> = []
    let sibling = marker.nextElementSibling

    while (isShellBlock(sibling)) {
        panes.push(sibling)
        sibling = sibling.nextElementSibling
    }

    if (panes.length < 2) {
        marker.remove()
        return
    }

    const wrapper = document.createElement('div')
    const toolbar = document.createElement('div')
    const tabs = document.createElement('div')
    let activeIndex = 0

    wrapper.className = 'code-block code-block--tabs'
    toolbar.className = 'code-toolbar'
    tabs.className = 'shell-tabs'
    tabs.setAttribute('role', 'tablist')
    tabs.setAttribute('aria-label', 'Shell')

    marker.before(wrapper)
    marker.remove()
    wrapper.append(toolbar)

    const buttons = panes.map((pane, paneIndex) => {
        const tab = document.createElement('button')
        const tabId = `shell-tab-${String(groupIndex)}-${String(paneIndex)}`
        const panelId = `shell-panel-${String(groupIndex)}-${String(paneIndex)}`

        tab.type = 'button'
        tab.id = tabId
        tab.className = 'shell-tabs__tab'
        tab.textContent = languageLabel(pane)
        tab.setAttribute('role', 'tab')
        tab.setAttribute('aria-controls', panelId)

        pane.id = panelId
        pane.dataset.codeEnhanced = 'true'
        pane.setAttribute('role', 'tabpanel')
        pane.setAttribute('aria-labelledby', tabId)
        wrapper.append(pane)

        return tab
    })

    const activate = (nextIndex: number, focus = false) => {
        activeIndex = nextIndex
        panes.forEach((pane, paneIndex) => {
            const active = paneIndex === activeIndex
            pane.hidden = !active
            buttons[paneIndex].setAttribute('aria-selected', String(active))
            buttons[paneIndex].tabIndex = active ? 0 : -1
        })

        if (focus) {
            buttons[activeIndex].focus()
        }
    }

    buttons.forEach((tab, tabIndex) => {
        tab.addEventListener('click', () => {
            activate(tabIndex)
        })
        tab.addEventListener('keydown', (event) => {
            let nextIndex: number

            if (event.key === 'ArrowRight') {
                nextIndex = (tabIndex + 1) % buttons.length
            } else if (event.key === 'ArrowLeft') {
                nextIndex = (tabIndex - 1 + buttons.length) % buttons.length
            } else if (event.key === 'Home') {
                nextIndex = 0
            } else if (event.key === 'End') {
                nextIndex = buttons.length - 1
            } else {
                return
            }

            event.preventDefault()
            activate(nextIndex, true)
        })
    })

    tabs.append(...buttons)
    const { announcement, button } = createCopyButton(() => panes[activeIndex])
    toolbar.append(tabs, button, announcement)
    activate(0)
}

export function CodeBlockControls() {
    useEffect(() => {
        const article = document.querySelector<HTMLElement>('.prose')
        if (!article) {
            return
        }

        article.querySelectorAll<HTMLElement>('[data-shell-tabs]').forEach((marker, index) => {
            enhanceShellGroup(marker, index)
        })

        article.querySelectorAll<HTMLPreElement>('pre[data-language]').forEach(enhanceStandalone)
    }, [])

    return null
}
