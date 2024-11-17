<script lang="ts">
  import { api } from "$lib/api/client";
  import { Button } from "$lib/components/ui/button/index.js";
  import { Input } from "$lib/components/ui/input/index.js";
  import Search from "lucide-svelte/icons/search";
  import Delete from "$lib/components/Delete";

  import type { RulesViewModel } from "$lib/api/models";

  import RuleText from "$lib/components/rules/ruleText.svelte";
  import { addTitlePart } from "$lib/title.svelte";

  let inputRef = $state<HTMLElement | null>(null);

  let currentRules = $state<RulesViewModel | null>(null);
  let validatedRules = $state<RulesViewModel | null>(null);

  let isRulesFileValid = $state<boolean | null>(null);

  api.GET("/rule").then((res) => {
    if (!res.error) currentRules = res.data;
  });

  const onchange = async (e: Event) => {
    const inputElement = e.target as HTMLInputElement;
    if (inputElement.files?.length !== 1) return;

    isRulesFileValid = null;
    const file = inputElement.files[0]!;
    const fileContents = await file.text();

    const potentiallyValidRules = JSON.parse(fileContents) as RulesViewModel;

    const res = await api.POST("/rule/validation", {
      body: potentiallyValidRules,
    });

    const isValid =
      !res.error && res.response.status === 200 && res.data.valueOf();

    isRulesFileValid = isValid;
    if (isValid) validatedRules = potentiallyValidRules;
  };

  const submit = async () => {
    if (validatedRules === null) return;

    const rules = await api.POST("/rule", {
      body: validatedRules,
    });

    currentRules = rules.data!;
    validatedRules = null;
    isRulesFileValid = null;
    if (inputRef !== null) (inputRef as HTMLInputElement).value = "";
  };

  const deleteRule = async (index: number) => {
    const rules = await api.DELETE("/rule/{index}", {
      params: {
        path: { index },
      },
    });

    currentRules = rules.data!;
  };

  addTitlePart("Rules");
</script>

<h1>Rules</h1>
<div>
  <Input type="file" {onchange} accept=".json" bind:ref={inputRef} />
  <Button
    class="font-bold"
    onclick={submit}
    disabled={validatedRules === null || isRulesFileValid === false}
  >
    Upload
  </Button>
  {#if isRulesFileValid !== null}
    {#if isRulesFileValid}
      ✅ Selected file is valid
    {:else}
      ❌ Selected file is not valid
    {/if}
  {/if}
  {#if currentRules}
    <div>
      <div>{currentRules.rules.length} Rules</div>
      {#each currentRules.rules as rule, i}
        <div class="flex flex-row items-center">
          <div>
            Rule {i}: <RuleText {...rule} />
          </div>
          <button onclick={() => deleteRule(i)}><Delete /></button>
          <div>
            <a href={`/search/rule:${i}`}><Search /></a>
          </div>
        </div>
      {/each}
    </div>
  {/if}
</div>
