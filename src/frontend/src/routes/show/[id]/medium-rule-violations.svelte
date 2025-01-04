<script lang="ts">
  import * as Card from "$lib/components/ui/card/index.js";
  import { api } from "$lib/api/client";

  import RuleText from "$lib/components/rules/ruleText.svelte";

  // Even though we don't use the `tags` prop, we still require it
  // so we can react to changes to its value.
  const { id }: { id: string; tags: string[] } = $props();

  let violatedRulesPromise = $derived.by(async () => {
    // Refresh when the parent's tags do.
    const res = await api.GET("/rule/violation/{mediumId}", {
      params: {
        path: {
          mediumId: id,
        },
      },
    });
    if (res.error) return null;
    return res.data.rules;
  });
</script>

<Card.Root>
  <Card.Content>
    <div>
      {#await violatedRulesPromise then violatedRules}
        {#if violatedRules !== null && violatedRules.length === 0}
          ✅
        {:else if violatedRules !== null}
          🚨
          {#each violatedRules as rule}
            <div>
              <RuleText {...rule} />
            </div>
          {/each}
        {/if}
      {/await}
    </div>
  </Card.Content>
</Card.Root>
