<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { ChevronRight } from '@lucide/vue'
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible'
import {
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarMenuSub,
  SidebarMenuSubButton,
  SidebarMenuSubItem,
} from '@/components/ui/sidebar'
import type { SessionYear } from '../types'
import { sessionLabel } from '../utils/sessionLabel'

const props = defineProps<{
  years: SessionYear[]
}>()

const route = useRoute()
const openYears = ref<number[]>([])
const openMeetings = ref<string[]>([])

function meetingId(year: number, meetingKey: number) {
  return `${year}:${meetingKey}`
}

function setOpen(items: number[], value: number, open: boolean): number[]
function setOpen(items: string[], value: string, open: boolean): string[]
function setOpen<T>(items: T[], value: T, open: boolean): T[] {
  return open ? [...new Set([...items, value])] : items.filter(item => item !== value)
}

watch(
  () => [route.params.sessionKey, props.years] as const,
  ([sessionKey, years]) => {
    for (const year of years) {
      for (const meeting of year.meetings) {
        if (meeting.sessions.some(session => String(session.sessionKey) === sessionKey)) {
          openYears.value = setOpen(openYears.value, year.year, true)
          openMeetings.value = setOpen(openMeetings.value, meetingId(year.year, meeting.meetingKey), true)
          return
        }
      }
    }
  },
  { immediate: true },
)
</script>

<template>
  <nav aria-label="Дерево сессий">
    <SidebarMenu>
      <Collapsible
        v-for="year in years"
        :key="year.year"
        :open="openYears.includes(year.year)"
        as-child
        @update:open="openYears = setOpen(openYears, year.year, $event)"
      >
        <SidebarMenuItem>
          <CollapsibleTrigger as-child>
            <SidebarMenuButton class="group">
              <ChevronRight class="transition-transform motion-reduce:transition-none group-data-[state=open]:rotate-90" />
              {{ year.year }}
            </SidebarMenuButton>
          </CollapsibleTrigger>
          <CollapsibleContent>
            <SidebarMenuSub>
              <Collapsible
                v-for="meeting in year.meetings"
                :key="meeting.meetingKey"
                :open="openMeetings.includes(meetingId(year.year, meeting.meetingKey))"
                as-child
                @update:open="openMeetings = setOpen(openMeetings, meetingId(year.year, meeting.meetingKey), $event)"
              >
                <SidebarMenuSubItem>
                  <CollapsibleTrigger as-child>
                    <SidebarMenuSubButton class="group">
                      <ChevronRight class="transition-transform motion-reduce:transition-none group-data-[state=open]:rotate-90" />
                      {{ meeting.name }}
                    </SidebarMenuSubButton>
                  </CollapsibleTrigger>
                  <CollapsibleContent>
                    <SidebarMenuSub>
                      <SidebarMenuSubItem
                        v-for="session in meeting.sessions"
                        :key="session.sessionKey"
                      >
                        <RouterLink
                          v-slot="{ href, navigate, isExactActive }"
                          :to="{ name: 'session', params: { sessionKey: session.sessionKey } }"
                          custom
                        >
                          <SidebarMenuSubButton as-child :is-active="isExactActive">
                            <a :href="href" @click="navigate">
                            {{ sessionLabel(session) }}
                            </a>
                          </SidebarMenuSubButton>
                        </RouterLink>
                      </SidebarMenuSubItem>
                    </SidebarMenuSub>
                  </CollapsibleContent>
                </SidebarMenuSubItem>
              </Collapsible>
            </SidebarMenuSub>
          </CollapsibleContent>
        </SidebarMenuItem>
      </Collapsible>
    </SidebarMenu>
  </nav>
</template>
