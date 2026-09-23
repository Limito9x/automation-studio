import { Link, useMatches } from "@tanstack/react-router"
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
} from "@/components/ui/breadcrumb"

export function AppBreadcrumb() {
  const matches = useMatches()

  // Filter matches that have a breadcrumb staticData
  const breadcrumbs = matches
    .filter((match) => (match.staticData as any)?.breadcrumb)
    .map((match) => {
      const rawBreadcrumb = (match.staticData as any).breadcrumb;
      const label = typeof rawBreadcrumb === "function" 
        ? rawBreadcrumb(match.params, match.loaderData) 
        : rawBreadcrumb;
      return {
        label,
        path: match.pathname,
      };
    })

  if (breadcrumbs.length === 0) return null;

  return (
    <Breadcrumb>
      <BreadcrumbList>
        <BreadcrumbItem>
          <BreadcrumbLink>
            <Link to="/" className="transition-colors hover:text-foreground text-xs">Application</Link>
          </BreadcrumbLink>
        </BreadcrumbItem>

        {breadcrumbs.map((item, index) => {
          const isLast = index === breadcrumbs.length - 1

          return (
            <BreadcrumbItem key={item.path}>
              {isLast ? (
                <BreadcrumbPage className="capitalize text-xs">{item.label}</BreadcrumbPage>
              ) : (
                <BreadcrumbLink>
                  <Link to={item.path} className="transition-colors hover:text-foreground text-xs">{item.label}</Link>
                </BreadcrumbLink>
              )}
            </BreadcrumbItem>
          )
        })}
      </BreadcrumbList>
    </Breadcrumb>
  )
}
