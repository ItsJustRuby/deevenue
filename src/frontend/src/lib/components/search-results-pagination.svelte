<script lang="ts">
  import type { SearchResultsViewModel } from "$lib/api/models";
  import * as Pagination from "$lib/components/ui/pagination/index.js";
  import { mediaQueries } from "$lib/utility";
  import ChevronLeft from "lucide-svelte/icons/chevron-left";
  import ChevronRight from "lucide-svelte/icons/chevron-right";
  import { MediaQuery } from "svelte/reactivity";

  interface Props {
    searchResults: SearchResultsViewModel;
    selectedPageNumber: number;
  }

  let { searchResults, selectedPageNumber = $bindable(1) }: Props = $props();

  const largeQuery = new MediaQuery(mediaQueries.largePagination);

  type Size = "large" | "small";
  let size: Size = $derived(largeQuery.current ? "large" : "small");
  let siblingCount = $derived(size === "large" ? 1 : 0);
</script>

<Pagination.Root
  count={searchResults.pageSize * searchResults.pageCount}
  perPage={searchResults.pageSize}
  {siblingCount}
  bind:page={selectedPageNumber}
>
  {#snippet children({ pages, currentPage })}
    {#if pages.length > 1}
      <Pagination.Content>
        <Pagination.Item>
          <Pagination.PrevButton disabled={currentPage === 1}>
            <ChevronLeft class="size-4" />
          </Pagination.PrevButton>
        </Pagination.Item>
        {#each pages as page (page.key)}
          {#if page.type === "ellipsis"}
            {#if size !== "small"}
              <Pagination.Item>
                <Pagination.Ellipsis />
              </Pagination.Item>
            {/if}
          {:else if size !== "small"}
            <Pagination.Item>
              <Pagination.Link {page} isActive={currentPage === page.value} />
            </Pagination.Item>
          {:else if currentPage === page.value}
            <Pagination.Item>
              <Pagination.Link {page} isActive={true} />
            </Pagination.Item>
          {/if}
        {/each}
        <Pagination.Item>
          <Pagination.NextButton
            disabled={currentPage === searchResults.pageCount}
          >
            <ChevronRight class="size-4" />
          </Pagination.NextButton>
        </Pagination.Item>
      </Pagination.Content>
    {/if}
  {/snippet}
</Pagination.Root>
