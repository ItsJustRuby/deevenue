<script lang="ts">
  import type { AllTagsViewModel, Rating, TagViewModel } from "$lib/api/models";
  import { api } from "$lib/api/client";
  import { Label } from "$lib/components/ui/label/index.js";
  import * as RadioGroup from "$lib/components/ui/radio-group/index.js";
  import * as Table from "$lib/components/ui/table/index.js";
  import { addTitlePart } from "$lib/title.svelte";

  let allTags = $state<AllTagsViewModel | null>(null);

  api.GET("/tag").then((res) => (allTags = res.data!));

  const onValueChange = async (i: number, tagName: string, rating: Rating) => {
    const res = await api.PUT("/tag/{tagName}/rating/{rating}", {
      params: {
        path: { tagName, rating },
      },
    });
    if (res.error) return;

    allTags!.tags[i] = res.data;
  };

  addTitlePart("Tags");
</script>

{#snippet ratingComponent(i: number, tag: TagViewModel)}
  <RadioGroup.Root
    class="grid-cols-3"
    value={tag.rating}
    onValueChange={(e) => onValueChange(i, tag.name, e as Rating)}
  >
    <div class="flex items-center space-x-2">
      <RadioGroup.Item value="safe" id="s" />
      <Label for="safe">s</Label>
    </div>
    <div class="flex items-center space-x-2">
      <RadioGroup.Item value="questionable" id="q" />
      <Label for="questionable">q</Label>
    </div>
    <div class="flex items-center space-x-2">
      <RadioGroup.Item value="explicit" id="e" />
      <Label for="explicit">e</Label>
    </div>
  </RadioGroup.Root>
{/snippet}

<h1>Tags</h1>
<div>
  {#if allTags !== null}
    <Table.Root class="w-full xl:w-4/5">
      <Table.Header>
        <Table.Row>
          <Table.Head class="min-w-24">Name</Table.Head>
          <Table.Head class="min-w-48">Rating</Table.Head>
          <Table.Head>#Media</Table.Head>
          <Table.Head>Aliases</Table.Head>
          <Table.Head>Implications</Table.Head>
        </Table.Row>
      </Table.Header>
      <Table.Body>
        {#each allTags.tags as t, i (t.name)}
          <Table.Row class={t.rating === "unknown" ? "bg-accent" : undefined}>
            <Table.Cell><a href="/tag/{t.name}">{t.name}</a></Table.Cell>
            <Table.Cell>{@render ratingComponent(i, t)}</Table.Cell>
            <Table.Cell>{t.mediaCount}</Table.Cell>
            <Table.Cell>{t.aliases.join(", ")}</Table.Cell>
            <Table.Cell
              >{t.impliedByThis.map((it) => `⇒ ${it}`).join(", ")}
              {t.implyingThis.map((it) => `${it} ⇒`).join(", ")}</Table.Cell
            >
          </Table.Row>
        {/each}
      </Table.Body>
    </Table.Root>
  {/if}
</div>
