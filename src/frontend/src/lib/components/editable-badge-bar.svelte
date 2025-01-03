<script lang="ts">
  import X from "lucide-svelte/icons/x";

  interface Props {
    onAdd?: (item: string) => Promise<void>;
    onRemove?: (item: string) => void;

    items: string[];
    getLink?: (item: string) => string;
  }

  let { getLink, items, onAdd, onRemove }: Props = $props();
  const sortedItems = $derived(items.toSorted());

  let currentName = $state("");

  const onkeypress = async (e: KeyboardEvent) => {
    const doSubmit =
      (e.code === "Enter" || e.code == "Space") &&
      !e.metaKey &&
      !e.shiftKey &&
      !e.altKey &&
      !e.ctrlKey;
    if (!doSubmit) return;

    e.preventDefault();
    await onAdd?.(currentName);
    currentName = "";
  };
</script>

<div
  class="flex flex-row flex-wrap space-x-1 rounded-md border-2 border-border p-1"
>
  {#each sortedItems as item}
    <div
      class="my-1 flex flex-row items-center rounded-full bg-black px-1 py-1 text-white"
    >
      {#if getLink}
        <a class="px-2 pb-1" href={getLink(item)}>{item}</a>
      {:else}
        <span class="px-2 pb-1">{item}</span>
      {/if}
      <button
        class="h-8 w-8 rounded-full bg-white px-1 text-black"
        onclick={() => onRemove?.(item)}
      >
        <div class="text-xs"><X /></div>
      </button>
    </div>
  {/each}
  <input
    bind:value={currentName}
    class="grow resize-none border-none focus:ring-0"
    {onkeypress}
    placeholder="Add…"
  />
</div>
