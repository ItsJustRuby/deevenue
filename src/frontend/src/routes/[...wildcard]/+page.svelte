<script lang="ts">
  import { goto } from "$app/navigation";

  // On 404, just go back to /.
  goto("/", { replaceState: true });
</script>
