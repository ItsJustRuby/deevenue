<script lang="ts">
  import { page } from "$app/stores";

  import { goto } from "$app/navigation";
  import { api } from "$lib/api/client";
  import type { SearchResultsViewModel } from "$lib/api/models";
  import { session, searchState, lastMediumUpdate } from "$lib/store.svelte";
  import SearchResultsPagination from "./search-results-pagination.svelte";
  import Thumbnail from "./thumbnail.svelte";
  import SearchResultsPageSizeSelector from "./search-results-page-size-selector.svelte";
  import { group } from "$lib/utils";

  let pageSize = $state(
    parseInt($page.url.searchParams.get("pageSize") ?? "10"),
  );

  let pageNumber = $state(
    parseInt($page.url.searchParams.get("pageNumber") ?? "1"),
  );

  const onPageSizeChange = (newPageSize: number) => {
    const indexOfFirstMediumOnPage = (pageNumber - 1) * pageSize;

    const newPageNumber =
      Math.floor(indexOfFirstMediumOnPage / newPageSize) + 1;

    pageSize = newPageSize;
    pageNumber = newPageNumber;
  };

  let searchParamsFromState = $derived.by(() => {
    var s = new URLSearchParams();
    if (pageNumber !== 1 || pageSize !== 10) {
      s.set("pageNumber", `${pageNumber}`);
      s.set("pageSize", `${pageSize}`);
    }
    return s;
  });

  $effect(() => {
    if ($page.url.searchParams.size === 0) {
      pageNumber = 1;
      pageSize = 10;
    }
  });

  $effect(() => {
    // This is the least "goto"-y "goto" ever.
    // It really should only updates the address bar, not impact the history or focus at all.
    goto(`?${searchParamsFromState.toString()}`, {
      keepFocus: true,
      replaceState: true,
    });
  });

  let searchResults = $state<SearchResultsViewModel | null>(null);

  const rerunSearch = async () => {
    if (session.isSfw === null || lastMediumUpdate.timestamp === null) {
      // This only happens during startup, which is irrelevant to us.
      return;
    }

    const res =
      searchState.searchTerm === ""
        ? await api.GET("/medium", {
            params: {
              query: {
                pageNumber,
                pageSize,
              },
            },
          })
        : await api.GET("/search", {
            params: {
              query: {
                q: searchState.searchTerm,
                pageNumber,
                pageSize,
              },
            },
          });

    searchResults = res.data!;
  };

  $effect(() => {
    rerunSearch();
  });
</script>

{#snippet headerAndFooter(searchResults: SearchResultsViewModel)}
  <div class="grid grid-cols-4">
    <div class="col-span-4 col-start-1 row-start-1 justify-self-start">
      <SearchResultsPagination
        {searchResults}
        bind:selectedPageNumber={pageNumber}
      />
    </div>
    <div class="col-start-4 row-start-1 justify-self-end">
      <SearchResultsPageSizeSelector
        initialValue={pageSize}
        {onPageSizeChange}
      />
    </div>
  </div>
{/snippet}

{#snippet columns(searchResults: SearchResultsViewModel, columnCount: number)}
  {#each group(searchResults.items, columnCount) as col}
    <div class="flex flex-col gap-4">
      {#each col as item}
        <div class="shadowy">
          <Thumbnail {...item} />
        </div>
      {/each}
    </div>
  {/each}
{/snippet}

<div class="space-y-4">
  {#if searchResults !== null}
    {#if searchResults.items.length === 0}
      <div class="text-4xl font-semibold">No results found.</div>
    {:else}
      {@render headerAndFooter(searchResults)}
      <div class="grid grid-cols-1 gap-4 lg:hidden">
        {@render columns(searchResults, 1)}
      </div>
      <div class="grid grid-cols-2 gap-4 max-lg:hidden xl:hidden">
        {@render columns(searchResults, 2)}
      </div>
      <div class="grid grid-cols-4 gap-4 max-xl:hidden">
        {@render columns(searchResults, 4)}
      </div>
      {@render headerAndFooter(searchResults)}
    {/if}
  {/if}
</div>

<style lang="postcss">
  .shadowy {
    box-shadow: 2px 2px 6px rgba(0, 0, 0, 0.6);
  }
</style>
