<script lang="ts">
  import { goto } from "$app/navigation";
  import { page } from "$app/state";

  import MediumTags from "./medium-tags.svelte";
  import MediumDisplay from "./medium-display.svelte";
  import MediumRuleViolations from "./medium-rule-violations.svelte";
  import type { MediumViewModel } from "$lib/api/models";
  import { api } from "$lib/api/client";
  import { session } from "$lib/store.svelte";
  import MediumAbsentTags from "./medium-absent-tags.svelte";
  import MediumDelete from "./medium-delete.svelte";
  import MediumThumbnailSheet from "./medium-thumbnail-sheet.svelte";
  import MediumSimilarMedia from "./medium-similar-media.svelte";
  import MediumRating from "./medium-rating.svelte";

  const id = $derived(page.params.id!);
  let medium = $state<MediumViewModel | null>(null);

  $effect(() => {
    if (session.isSfw && medium !== null && medium.rating !== "safe") goto("/");
  });

  $effect(() => {
    // This is mainly here so svelte will know to rerun
    // this $effect when session.isSfw changes :/
    if (session.isSfw === null) return;

    api
      .GET("/medium/{id}", {
        params: {
          path: {
            id,
          },
        },
      })
      .then(async (res) => {
        if (!res.error) {
          medium = res.data;
          return;
        }

        if (res.response.status === 404 || res.response.status === 403) {
          await goto("/", { replaceState: true });
          return;
        }
      });
  });
</script>

<div>
  {#if medium !== null}
    <MediumDisplay {...medium} />
    <div class="flex flex-col space-y-2">
      <MediumTags bind:medium />
      <MediumAbsentTags bind:medium />
      <MediumSimilarMedia {...medium} />
      <div class="flex flex-row space-x-2">
        <MediumRating bind:medium />
        <MediumRuleViolations id={medium.id} tags={medium.tags} />
      </div>

      {#if medium.mediaKind === "video"}
        <MediumThumbnailSheet
          id={medium.id}
          thumbnailSheetIds={medium.thumbnailSheetIds}
        />
      {/if}
      <MediumDelete id={medium.id} />
    </div>
  {/if}
</div>
