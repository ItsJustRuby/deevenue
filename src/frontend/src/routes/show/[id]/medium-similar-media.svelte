<script lang="ts">
  import type { MediumViewModel } from "$lib/api/models";
  import Thumbnail from "$lib/components/thumbnail.svelte";

  const { similarMedia }: Pick<MediumViewModel, "similarMedia"> = $props();
</script>

<div
  class="md-max:grid-rows-5 grid justify-between justify-items-center gap-1 md:grid-cols-5"
>
  {#each similarMedia as m}
    <div class="w-fit" style="height: inherit">
      <Thumbnail {...m} />
    </div>
  {/each}
</div>
