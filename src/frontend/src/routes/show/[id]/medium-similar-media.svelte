<script lang="ts">
  import { api } from "$lib/api/client";
  import type { MediumViewModel } from "$lib/api/models";
  import Spinner from "$lib/components/spinner.svelte";
  import Thumbnail from "$lib/components/thumbnail.svelte";

  const { id }: Pick<MediumViewModel, "id"> = $props();

  const similarMediaPromise = $derived.by(async () => {
    const res = await api.GET("/medium/{id}/similar", {
      params: {
        path: {
          id,
        },
      },
    });

    if (res.error) return [];

    return res.data.similarMedia;
  });
</script>

<div
  class="md-max:grid-rows-5 grid justify-between justify-items-center gap-1 md:grid-cols-5"
>
  {#await similarMediaPromise}
    <Spinner class="size-1/2" />
  {:then similarMedia}
    {#each similarMedia as m}
      <div class="w-fit" style="height: inherit">
        <Thumbnail {...m} />
      </div>
    {/each}
  {/await}
</div>
