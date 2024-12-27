<script lang="ts">
  import * as Card from "$lib/components/ui/card/index.js";
  import { Label } from "$lib/components/ui/label/index.js";
  import { Switch } from "$lib/components/ui/switch/index.js";

  import { onMount } from "svelte";

  import Spinner from "$lib/components/spinner.svelte";
  import { api } from "$lib/api/client";
  import type { JobsViewModel } from "$lib/api/models";
  import { addTitlePart } from "$lib/title.svelte";
  import JobResults from "./job-results.svelte";
  import { group } from "$lib/utils";

  let doAutoRefresh = $state<boolean>(true);
  let autoRefreshInterval = $state<ReturnType<typeof setInterval> | null>(null);
  let runningJobs = $state<JobsViewModel | null>(null);

  const getRunningJobs = async () => {
    if (!doAutoRefresh) return Promise.resolve();

    const res = await api.GET("/job/running");

    if (res.error) return;
    runningJobs = res.data;
  };

  onMount(() => {
    getRunningJobs().then(() => {
      autoRefreshInterval = setInterval(() => getRunningJobs(), 1000);
    });

    return () => {
      if (autoRefreshInterval) clearInterval(autoRefreshInterval);
    };
  });

  addTitlePart("Jobs");
</script>

{#snippet columns(jobGroup: JobsViewModel, columnCount: number)}
  {#each group(jobGroup.kinds, columnCount) as g}
    <div class="space-y-4">
      {#each g as kind}
        <Card.Root>
          <Card.Header>
            <Card.Title>{kind.kind}</Card.Title>
            <Card.Description>{kind.jobs.length} running jobs</Card.Description>
          </Card.Header>
          <Card.Content>
            <ul>
              {#each kind.jobs as job}
                <li>
                  {#if job.mediumId}
                    <a href={`/show/${job.mediumId}`}>Medium {job.mediumId}</a>
                  {:else}
                    <p>Running…</p>
                  {/if}
                </li>
              {/each}
            </ul>
          </Card.Content>
        </Card.Root>
      {/each}
    </div>
  {/each}
{/snippet}

<h1>Jobs</h1>
<h2>Running</h2>
<div class="mt-2 pb-4">
  <div
    class="flex w-fit select-none flex-row items-center justify-items-start gap-1"
  >
    <Switch name="autoRefresh" id="autoRefresh" bind:checked={doAutoRefresh} />
    <Label for="autoRefresh">Auto refresh</Label>
    <Spinner class={doAutoRefresh ? "visible" : "invisible"} />
  </div>
  {#if runningJobs}
    <div class="pt-4">
      <div class="grid grid-cols-1 gap-4 lg:hidden">
        {@render columns(runningJobs, 1)}
      </div>
      <div class="grid grid-cols-2 gap-4 max-lg:hidden xl:hidden">
        {@render columns(runningJobs, 2)}
      </div>
      <div class="grid grid-cols-4 gap-4 max-xl:hidden">
        {@render columns(runningJobs, 4)}
      </div>
    </div>
  {/if}
</div>
<h2>Completed</h2>
<JobResults />
