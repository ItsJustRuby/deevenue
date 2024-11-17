<script lang="ts" generics="TItem">
  import type { Snippet } from "svelte";

  interface Props<T> {
    child?: Snippet<[T]>;
    items: T[];

    finalSeparator?: string;
  }

  // eslint-disable-next-line no-undef
  const { child, items, finalSeparator = "and" }: Props<TItem> = $props();
</script>

{#each items as item, i}
  {#if i > 0 && i < items.length - 1}
    ,{" "}
  {:else if items.length > 1 && i == items.length - 1}
    {` ${finalSeparator} `}
  {:else}
    {" "}
  {/if}
  {#if child}
    {@render child(item)}
  {:else}
    {item}
  {/if}
{/each}
