<script lang="ts">
  import * as Card from "$lib/components/ui/card/index.js";
  import type { RuleViewModel } from "$lib/api/models";
  import { api } from "$lib/api/client";

  import RuleText from "$lib/components/rules/ruleText.svelte";

  // Even though we don't use the `tags` prop, we still require it
  // so we can react to changes to its value.
  const { id }: { id: string; tags: string[] } = $props();

  let violatedRules = $state<RuleViewModel[] | null>(null);

  // Refresh when the parent's tags do.
  $effect(() => {
    api
      .GET("/rule/violation/{mediumId}", {
        params: {
          path: {
            mediumId: id,
          },
        },
      })
      .then((res) => {
        if (res.error) return;
        violatedRules = res.data.rules;
      });
  });
</script>

<Card.Root>
  <Card.Content>
    <div>
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
    </div>
  </Card.Content>
</Card.Root>
