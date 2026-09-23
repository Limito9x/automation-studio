import * as React from "react"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { cn } from "@/lib/utils"
import { ImageIcon } from "lucide-react"

export interface BaseCardProps extends React.ComponentPropsWithoutRef<typeof Card> {
  title: string
  description?: string
  icon?: React.ElementType
  thumbnailUrl?: string
  fallbackThumbnail?: React.ReactNode
  showThumbnail?: boolean
  footer?: React.ReactNode
  action?: React.ReactNode
  onClick?: () => void
}

export const BaseCard = React.forwardRef<HTMLDivElement, BaseCardProps>(
  ({ className, title, description, icon: Icon, thumbnailUrl, fallbackThumbnail, showThumbnail = true, footer, action, onClick, children, ...props }, ref) => {
    const [imgError, setImgError] = React.useState(false);

    return (
      <Card 
        ref={ref} 
        className={cn(
          "group flex flex-col h-full overflow-hidden transition-all duration-200 hover:border-primary/50 hover:shadow-md", 
          onClick && "cursor-pointer", 
          className
        )} 
        onClick={onClick}
        {...props}
      >
        {showThumbnail && (
          <div className="relative w-full aspect-video bg-gradient-to-br from-muted/60 via-muted/30 to-muted/10 overflow-hidden border-b flex items-center justify-center shrink-0">
            {thumbnailUrl && !imgError ? (
              <img 
                src={thumbnailUrl} 
                alt={title} 
                className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-105"
                onError={() => setImgError(true)}
              />
            ) : fallbackThumbnail ? (
              fallbackThumbnail
            ) : (
              <div className="flex flex-col items-center justify-center text-muted-foreground/40 gap-1.5 transition-colors group-hover:text-muted-foreground/60">
                <div className="w-10 h-10 rounded-full bg-background/60 border flex items-center justify-center shadow-xs">
                  <ImageIcon className="h-5 w-5" />
                </div>
                <span className="text-[0.65rem] font-medium tracking-wider uppercase opacity-60">No Image</span>
              </div>
            )}
          </div>
        )}

        <CardHeader className="flex flex-row items-start justify-between space-y-0 p-4 pb-2">
          <div className="flex items-center space-x-2.5 min-w-0 flex-1 pr-2">
            {Icon && (
              <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-md bg-primary/10 text-primary">
                <Icon className="h-4 w-4" />
              </div>
            )}
            <div className="space-y-1 min-w-0 flex-1">
              <CardTitle className="text-sm font-semibold truncate group-hover:text-primary transition-colors" title={title}>
                {title}
              </CardTitle>
            </div>
          </div>
          {action && (
            <div className="ml-auto shrink-0" onClick={(e) => e.stopPropagation()}>
              {action}
            </div>
          )}
        </CardHeader>

        <CardContent className="flex-1 px-4 pb-4">
          <CardDescription className="text-xs text-muted-foreground line-clamp-2 min-h-[2.25rem]">
            {description || <span className="italic opacity-40">No description</span>}
          </CardDescription>
          {children}
        </CardContent>

        {footer && (
          <CardFooter className="bg-muted/20 px-4 py-2 text-xs text-muted-foreground border-t mt-auto">
            {footer}
          </CardFooter>
        )}
      </Card>
    )
  }
)

BaseCard.displayName = "BaseCard"
