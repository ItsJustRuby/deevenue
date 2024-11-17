<script lang="ts">
  import Delete from "$lib/components/Delete";
  import * as Table from "$lib/components/ui/table/index.js";

  import type { JobResultViewModel } from "$lib/api/models";
  import { api } from "$lib/api/client";
  import { onMount } from "svelte";

  let pastJobResults = $state<JobResultViewModel[] | null>(null);

  onMount(() => {
    api.GET("/job/result").then((res) => {
      if (res.error) return;
      pastJobResults = res.data.results;
    });
  });

  const remove = async (id: string) => {
    const res = await api.DELETE("/job/result/{id}", {
      params: {
        path: {
          id,
        },
      },
    });

    if (res.error) return;

    pastJobResults = pastJobResults!.filter((r) => r.id != id);
  };
</script>

<div class="mt-2">
  {#if pastJobResults !== null}
    {#if pastJobResults.length > 0}
      <Table.Root class="w-full">
        <Table.Header>
          <Table.Row>
            <Table.Head>Timestamp</Table.Head>
            <Table.Head>Job kind</Table.Head>
            <Table.Head>Job ID</Table.Head>
            <Table.Head class="w-72">Error text</Table.Head>
            <Table.Head></Table.Head>
          </Table.Row>
        </Table.Header>
        <Table.Body>
          {#each pastJobResults as result}
            <Table.Row>
              <Table.Cell
                >{new Date(result.insertedAt).toISOString()}</Table.Cell
              >
              <Table.Cell>{result.jobKindName}</Table.Cell>
              <Table.Cell>{result.jobId}</Table.Cell>
              <Table.Cell>{result.errorText}</Table.Cell>
              <Table.Cell
                ><button onclick={() => remove(result.id)}>
                  <Delete />
                </button></Table.Cell
              >
            </Table.Row>
          {/each}
        </Table.Body>
      </Table.Root>
    {:else}
      ✅ No failures found
    {/if}
  {/if}
</div>
