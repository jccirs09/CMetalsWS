import { Badge } from "./ui/badge";
import { cn } from "./ui/utils";

export type Status = 
  | "planned" 
  | "ready" 
  | "in-progress" 
  | "paused" 
  | "completed" 
  | "error"
  | "on-time"
  | "delayed"
  | "cancelled"
  | "picking"
  | "picked"
  | "packed"
  | "shipped"
  | "pending";

interface StatusChipProps {
  status: Status;
  className?: string;
  size?: "sm" | "default" | "lg";
}

const statusConfig = {
  pending: {
    variant: "secondary" as const,
    className: "bg-slate-100 text-slate-700 hover:bg-slate-200",
    label: "Pending"
  },
  planned: {
    variant: "secondary" as const,
    className: "bg-gray-100 text-gray-700 hover:bg-gray-200",
    label: "Planned"
  },
  ready: {
    variant: "default" as const,
    className: "bg-blue-100 text-blue-700 hover:bg-blue-200",
    label: "Ready"
  },
  "in-progress": {
    variant: "default" as const,
    className: "bg-teal-100 text-teal-700 hover:bg-teal-200",
    label: "In Progress"
  },
  paused: {
    variant: "default" as const,
    className: "bg-amber-100 text-amber-700 hover:bg-amber-200",
    label: "Paused"
  },
  completed: {
    variant: "default" as const,
    className: "bg-green-100 text-green-700 hover:bg-green-200",
    label: "Completed"
  },
  error: {
    variant: "destructive" as const,
    className: "bg-red-100 text-red-700 hover:bg-red-200",
    label: "Error"
  },
  "on-time": {
    variant: "default" as const,
    className: "bg-green-100 text-green-700 hover:bg-green-200",
    label: "On Time"
  },
  delayed: {
    variant: "default" as const,
    className: "bg-red-100 text-red-700 hover:bg-red-200",
    label: "Delayed"
  },
  cancelled: {
    variant: "secondary" as const,
    className: "bg-gray-100 text-gray-700 hover:bg-gray-200",
    label: "Cancelled"
  },
  picking: {
    variant: "default" as const,
    className: "bg-yellow-100 text-yellow-700 hover:bg-yellow-200",
    label: "Picking"
  },
  picked: {
    variant: "default" as const,
    className: "bg-blue-100 text-blue-700 hover:bg-blue-200",
    label: "Picked"
  },
  packed: {
    variant: "default" as const,
    className: "bg-purple-100 text-purple-700 hover:bg-purple-200",
    label: "Packed"
  },
  shipped: {
    variant: "default" as const,
    className: "bg-green-100 text-green-700 hover:bg-green-200",
    label: "Shipped"
  }
};

export function StatusChip({ status, className, size = "default" }: StatusChipProps) {
  const config = statusConfig[status];
  
  if (!config) {
    console.warn(`Unknown status: ${status}`);
    return <Badge variant="secondary" className={className}>Unknown</Badge>;
  }
  
  const sizeClasses = {
    sm: "text-xs px-2 py-0.5",
    default: "text-sm px-2.5 py-0.5",
    lg: "text-base px-3 py-1"
  };
  
  return (
    <Badge 
      variant={config.variant}
      className={cn(config.className, sizeClasses[size], className)}
    >
      {config.label}
    </Badge>
  );
}