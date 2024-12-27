<script lang="ts">
  import Dismiss from "lucide-svelte/icons/x";
  import {
    notifications,
    dismissNotification,
  } from "$lib/notifications/store.svelte";

  // By reversing the notifications in HTML, then reversing how they are displayed again in CSS,
  // you get free "scroll to the bottom" behavior when a new notification is added.
  let reverseNotifications = $derived(notifications.toReversed());
</script>

<div class="flex max-h-48 flex-col-reverse overflow-scroll overscroll-none">
  {#each reverseNotifications as notification, i}
    <div
      class="mb-2 flex h-12 min-h-12 w-full flex-row rounded-md pl-4 pr-2 shadow-md"
      data-level={notification.level}
    >
      <div class="self-center">
        [{notification.timestamp.toLocaleTimeString()}]
        {#each notification.contents as part}
          {#if part.type === "text"}
            <span>{part.text}</span>
          {:else if part.type === "link"}
            <a href={part.location}>{part.text}</a>
          {/if}
        {/each}
      </div>
      <div class="grow"></div>
      <div class="mt-1 h-full">
        <button onclick={() => dismissNotification(i)}><Dismiss /></button>
      </div>
    </div>
  {/each}
</div>

<style lang="postcss">
  [data-level="info"] {
    @apply bg-secondary;
  }

  [data-level="warning"] {
    @apply bg-primary;
  }

  [data-level="error"] {
    @apply bg-destructive;
  }
</style>
