<script lang="ts">
  import { page } from "$app/stores";
  import SearchResults from "$lib/components/search-results.svelte";
  import { searchState } from "$lib/store.svelte";
  import { addTitlePart } from "$lib/title.svelte";

  $effect(() => {
    searchState.searchTerm = $page.params.query!.replaceAll("_", " ");
    addTitlePart(searchState.searchTerm);
  });
</script>

<div>
  <SearchResults />
</div>
