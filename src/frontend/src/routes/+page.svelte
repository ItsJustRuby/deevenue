<script lang="ts">
  import { searchState } from "$lib/store.svelte";
  import SearchResults from "$lib/components/search-results.svelte";

  // Reset search term when you intentionally land on "/"
  searchState.searchTerm = "";
</script>

<SearchResults />
