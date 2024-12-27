<script lang="ts">
  import type { IRuleThen } from "$lib/api/models";
  import JoinedParts from "./joinedParts.svelte";
  import ThenText from "./thenText.svelte";

  const { thens }: { thens: IRuleThen[] } = $props();
</script>

, then it <JoinedParts items={thens}>
  {#snippet child(then)}<ThenText {...then} />
  {/snippet}</JoinedParts
>
