<script lang="ts">
  import type { SearchResultsViewModel } from "$lib/api/models";
  import * as Pagination from "$lib/components/ui/pagination/index.js";
  import ChevronLeft from "lucide-svelte/icons/chevron-left";
  import ChevronRight from "lucide-svelte/icons/chevron-right";

  interface Props {
    searchResults: SearchResultsViewModel;
    selectedPageNumber: number;
  }

  let { searchResults, selectedPageNumber = $bindable(1) }: Props = $props();
</script>

<Pagination.Root
  count={searchResults.pageSize * searchResults.pageCount}
  perPage={searchResults.pageSize}
  siblingCount={1}
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
            <Pagination.Item>
              <Pagination.Ellipsis />
            </Pagination.Item>
          {:else}
            <Pagination.Item>
              <Pagination.Link {page} isActive={currentPage === page.value} />
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
