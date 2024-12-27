<script lang="ts">
  import type { IRuleIff } from "$lib/api/models";
  import IfText from "./ifText.svelte";
  import JoinedParts from "./joinedParts.svelte";

  const { ifs }: { ifs: IRuleIff[] } = $props();
</script>

If a medium
<JoinedParts items={ifs}>
  {#snippet child(iff)}<IfText {...iff} />
  {/snippet}</JoinedParts
>
