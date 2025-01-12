<script lang="ts">
  import { page } from "$app/state";
  import SearchResults from "$lib/components/search-results.svelte";
  import { searchState } from "$lib/store.svelte";
  import { addTitlePart } from "$lib/title.svelte";

  let isSearchTermPropagated = $state(false);

  $effect(() => {
    searchState.searchTerm = page.params.query!.replaceAll("_", " ");
    isSearchTermPropagated = true;
    addTitlePart(searchState.searchTerm);
    return () => {
      isSearchTermPropagated = false;
    };
  });
</script>

{#if isSearchTermPropagated}
  <SearchResults />
{/if}
