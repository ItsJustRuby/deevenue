<script lang="ts">
  import { goto } from "$app/navigation";
  import { getRandomRuleViolation } from "$lib/randomRuleViolation";
  import { onMount } from "svelte";

  onMount(async () => {
    const res = await getRandomRuleViolation();
    if (res !== null) await goto(res);
  });
</script>
