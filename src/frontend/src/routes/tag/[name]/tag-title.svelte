<script lang="ts">
  import { goto } from "$app/navigation";
  import { api } from "$lib/api/client";
  import type { TagViewModel } from "$lib/api/models";
  import { Input } from "$lib/components/ui/input/index.js";

  interface Props {
    tag: TagViewModel;
  }

  let { tag = $bindable() }: Props = $props();

  let currentName = $state(tag.name);
  let isEditing = $state(false);
  let inputRef = $state<HTMLElement | null>(null);

  const onkeypress = async (e: KeyboardEvent) => {
    const doSubmit =
      e.code === "Enter" &&
      !e.metaKey &&
      !e.shiftKey &&
      !e.altKey &&
      !e.ctrlKey;
    if (!doSubmit) return;

    e.preventDefault();
    return onblur();
  };

  const onblur = async () => {
    isEditing = false;

    if (currentName !== tag.name) {
      const res = await api.PATCH("/tag/{currentName}/name/{newName}", {
        params: {
          path: {
            currentName: tag.name,
            newName: currentName,
          },
        },
      });

      if (res.error || res.response.status === 404) {
        currentName = tag.name;
        return;
      }

      tag = res.data;
      currentName = tag.name;

      return goto(`/tag/${currentName}`, {
        replaceState: true,
      });
    }
  };

  const onGlobalHotkey = async (e: KeyboardEvent) => {
    if (
      document.activeElement?.tagName === "body".toUpperCase() &&
      !isEditing &&
      e.key === "e"
    ) {
      e.preventDefault();
      isEditing = true;
    }
  };

  $effect(() => {
    if (isEditing && inputRef !== null && document.activeElement !== inputRef) {
      // This is just reimplementing autofocus without the a11y warning.
      // Not very smart.
      inputRef.focus();
    }
  });
</script>

<svelte:window onkeypress={onGlobalHotkey} />

{#if isEditing}
  <Input
    type="text"
    bind:ref={inputRef}
    bind:value={currentName}
    {onblur}
    {onkeypress}
  />
{:else}
  <button
    class="cursor-text scroll-m-20 text-4xl font-extrabold tracking-tight lg:text-5xl"
    onclick={() => (isEditing = true)}
  >
    "{currentName}"
  </button>
{/if}
