<script lang="ts">
  import * as Card from "$lib/components/ui/card/index.js";
  import { api } from "$lib/api/client";
  import type {
    MediumViewModel,
    ThumbnailSheetViewModel,
  } from "$lib/api/models";
  import { Button } from "$lib/components/ui/button/index.js";
  import { Skeleton } from "$lib/components/ui/skeleton/index.js";
  import { Slider } from "$lib/components/ui/slider/index.js";
  import { Label } from "$lib/components/ui/label/index.js";
  import ThumbnailPicture from "$lib/components/thumbnail-picture.svelte";

  const {
    id,
    thumbnailSheetIds,
  }: Pick<MediumViewModel, "id" | "thumbnailSheetIds"> = $props();

  let currentThumbnailSheet = $state<ThumbnailSheetViewModel | null>(null);

  let count = $state([10]);

  let pendingThumbnailSheetId = $state<string | null>(null);
  let pendingThumbnailSheetIntervalToClear = $state<number | null>(null);

  const requestThumbnailSheet = async () => {
    const res = await api.POST("/medium/{id}/thumbnailsheets", {
      body: {
        thumbnailCount: count[0]!,
      },
      params: {
        path: {
          id,
        },
      },
    });

    if (res.error) return;
    startPollingForId(res.data.id);
  };

  const pollPendingThumbnailSheet = async () => {
    const res = await api.GET("/thumbnailsheet/{id}", {
      params: {
        path: { id: pendingThumbnailSheetId! },
      },
    });

    if (res.error) return;

    if (res.data.isComplete) {
      currentThumbnailSheet = res.data;
      clearInterval(pendingThumbnailSheetIntervalToClear!);
      pendingThumbnailSheetId = null;
      pendingThumbnailSheetIntervalToClear = null;
    }
  };

  if (thumbnailSheetIds.length > 0) {
    api
      .GET("/thumbnailsheet/{id}", {
        params: {
          path: {
            id: thumbnailSheetIds[0]!,
          },
        },
      })
      .then((res) => {
        if (res.error) return;
        if (res.data.isComplete) currentThumbnailSheet = res.data;
        else startPollingForId(res.data.id);
      });
  }

  const startPollingForId = (id: string) => {
    pendingThumbnailSheetId = id;
    pendingThumbnailSheetIntervalToClear = window.setInterval(
      pollPendingThumbnailSheet,
      1000,
    );
  };

  const reject = async () => {
    await api.DELETE("/thumbnailsheet/{id}", {
      params: {
        path: {
          id: currentThumbnailSheet!.id,
        },
      },
    });

    currentThumbnailSheet = null;
  };

  const select = async (i: number) => {
    await api.POST("/thumbnailsheet/{id}/selection/{thumbnailIndex}", {
      params: {
        path: {
          id: currentThumbnailSheet!.id,
          thumbnailIndex: i,
        },
      },
    });

    // Assume everything went well
    currentThumbnailSheet = null;
  };
</script>

<Card.Root>
  <Card.Content>
    <div class="grid gap-4">
      {#if currentThumbnailSheet === null}
        {#if pendingThumbnailSheetId === null}
          <div class="grid grid-cols-1 gap-4">
            <div class="flex flex-row gap-4">
              <Label for="thumbnailCount">#Thumbnails: {count}</Label>
              <Slider
                id="thumbnailCount"
                class="w-64"
                bind:value={count}
                min={5}
                max={100}
                step={5}
              />
            </div>
            <Button class="font-bold" onclick={requestThumbnailSheet}>
              Request thumbnail sheet
            </Button>
          </div>
        {:else}
          <div class="flex flex-row flex-wrap">
            <!-- eslint-disable-next-line @typescript-eslint/no-unused-vars -->
            {#each [...Array(count[0]).keys()] as _}
              <div class="min-w-64 px-1 py-1">
                <Skeleton class="h-32 w-full rounded-md" />
              </div>
            {/each}
          </div>
        {/if}
      {:else}
        <div>
          <div class="flex flex-row flex-wrap gap-4">
            {#each [...Array(currentThumbnailSheet.count).keys()] as i}
              <div
                class="w-52 cursor-pointer transition duration-100 ease-in-out hover:scale-110 hover:drop-shadow-2xl"
              >
                <button onclick={() => select(i)}>
                  <ThumbnailPicture
                    prefix={`/thumbnailsheet/${currentThumbnailSheet.id}/${i}`}
                  />
                </button>
              </div>
            {/each}
          </div>
          <div>
            <Button class="font-bold" onclick={reject}
              >Reject thumbnail sheet</Button
            >
          </div>
        </div>
      {/if}
    </div>
  </Card.Content>
</Card.Root>
