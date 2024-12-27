<script lang="ts">
  import type { IRuleIff } from "$lib/api/models";
  import JoinedParts from "./joinedParts.svelte";
  import TagLink from "./tagLink.svelte";

  const iff: IRuleIff = $props();
</script>

{#if iff.type === "all"}
  exists
{:else if iff.type === "hasAnyTagsIn"}
  has any of the tags
  <JoinedParts items={iff.tags} finalSeparator="or">
    {#snippet child(tag)}
      <TagLink {tag} />
    {/snippet}
  </JoinedParts>
{:else if iff.type === "hasAnyTagsLike"}
  has a tag that matches the regexes <JoinedParts items={iff.regexes} />
{:else if iff.type === "hasRating"}
  has the rating {iff.rating}
{/if}
