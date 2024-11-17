<script lang="ts">
  import type { RuleViewModel } from "$lib/api/models";
  import IfsText from "./ifsText.svelte";
  import ThensText from "./thensText.svelte";

  const rule: RuleViewModel = $props();
</script>

<IfsText ifs={rule.iffs!} /><ThensText thens={rule.thens!} />.
