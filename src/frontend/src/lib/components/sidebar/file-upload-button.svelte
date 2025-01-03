<script lang="ts">
  import ImageUp from "lucide-svelte/icons/image-up";
  import { Button } from "$lib/components/ui/button/index.js";
  import { Input } from "$lib/components/ui/input/index.js";
  import { Label } from "$lib/components/ui/label/index.js";
  import { Progress } from "$lib/components/ui/progress/index.js";
  import { api } from "$lib/api/client";
  import { goto } from "$app/navigation";
  import { lastMediumUpdate } from "$lib/store.svelte";
  import { getEntityFromNotification } from "$lib/notifications";
  import type { ChangeEventHandler } from "svelte/elements";

  let fileCount = $state(0);
  let uploadedFileCount = $state(0);
  let inputRef = $state<HTMLInputElement | null>(null);

  const submit = async (e: Event) => {
    e.preventDefault();
    const formData = new FormData((e.target as HTMLButtonElement).form!);

    const formFiles = formData.getAll("files") as File[];
    fileCount = formFiles.length;

    const upload = async (file: File) => {
      const fakeFormData = new FormData();
      fakeFormData.append("file", file);

      return api
        .POST("/medium", {
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          body: fakeFormData as any,
        })
        .then(async (res) => {
          uploadedFileCount += 1;

          // Note that this covers both 200 and 409 :)
          return {
            isSuccess: !res.error,
            medium: getEntityFromNotification(res.data || res.error),
          };
        });
    };

    await Promise.all(formFiles.map(upload)).then((results) => {
      if (results.some((r) => r.isSuccess) && results.length > 1)
        lastMediumUpdate.timestamp = new Date();

      // Only reset after a slight delay so the UI is less flickery
      // (e.g. the progress bar isn't just gone)
      setTimeout(() => {
        fileCount = 0;
        uploadedFileCount = 0;
        if (inputRef !== null) inputRef.value = "";
      }, 1000);

      if (results.length === 1 && results[0] !== undefined) {
        return goto(`/show/${results[0].medium!.id}`);
      }
    });
  };

  const setFileCount: ChangeEventHandler<HTMLInputElement> = (e) => {
    fileCount = e.currentTarget.files?.length || 0;
  };
</script>

<div class="rounded-lg border bg-card p-2">
  <form class="grid gap-2">
    <div class="grid grid-cols-1 grid-rows-1 hover:bg-accent">
      <Input
        class=" col-start-1 row-start-1 cursor-pointer opacity-0 "
        type="file"
        multiple
        name="files"
        id="files"
        onchange={setFileCount}
        bind:ref={inputRef}
      />
      <div class="col-start-1 row-start-1 grid place-items-center border">
        <ImageUp />
      </div>
    </div>

    <Button class="font-bold" onclick={submit} disabled={fileCount === 0}>
      Upload
    </Button>
    <div
      class:invisible={fileCount === 0}
      class="grid h-12 min-h-12 grid-rows-2"
    >
      <Label>Upload progress ({uploadedFileCount} / {fileCount})</Label>
      <!-- The "|| 1" fixes annoying CSS animation behavior if both
     value and max start off as 0. -->
      <Progress max={fileCount || 1} value={uploadedFileCount} />
    </div>
  </form>
</div>
