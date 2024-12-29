export const mediaQueries = {
  largePagination: "(min-width: 480px)",
  largeThumbnail: "(min-width: 1024px)",
};

export const backendUrl = (remainder: string) => {
  return "/api" + remainder;
};

export const group = <T>(items: T[], columnCount: number): T[][] => {
  const k = Math.ceil(items.length / columnCount);
  const results: T[][] = [];
  for (let columnIndex = 0; columnIndex < columnCount; columnIndex++) {
    results.push(items.slice(k * columnIndex, k * (columnIndex + 1)));
  }
  return results;
};
