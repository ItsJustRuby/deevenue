<script lang="ts">
  import type { IRuleThen } from "$lib/api/models";
  import JoinedParts from "./joinedParts.svelte";
  import TagLink from "./tagLink.svelte";

  const then: IRuleThen = $props();
</script>

{#if then.type === "fail"}
  should not exist
{:else if then.type === "hasRating"}
  should have the rating {then.rating}
{:else if then.type === "hasAnyRating"}
  should have a rating
{:else if then.type === "hasAnyTagsIn"}
  should have any of the tags
  <JoinedParts items={then.tags} finalSeparator="or">
    {#snippet child(tag)}
      <TagLink {tag} />
    {/snippet}
  </JoinedParts>
{:else if then.type === "hasAnyTagsLike"}
  should have a tag that matches any the regexes <JoinedParts
    items={then.regexes}
  />
{:else if then.type === "hasAllAbsentOrPresent"}
  should have all of the tags <JoinedParts items={then.tags}>
    {#snippet child(tag)}
      <TagLink {tag} />
    {/snippet}
  </JoinedParts> marked as absent or present
{/if}
