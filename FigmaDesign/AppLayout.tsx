import { useState } from "react";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Input } from "./ui/input";
import {
  Sidebar,
  SidebarContent,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
  SidebarTrigger,
} from "./ui/sidebar";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "./ui/dropdown-menu";
import {
  LayoutDashboard,
  Calendar,
  CalendarClock,
  FileText,
  FilePlus,
  PlayCircle,
  Package,
  Warehouse,
  Truck,
  Users,
  Settings,
  BarChart3,
  Search,
  Bell,
  User,
  Menu,
  ChevronDown
} from "lucide-react";

interface AppLayoutProps {
  children: React.ReactNode;
  currentPage?: string;
  userRole?: "manager" | "supervisor" | "planner" | "operator" | "driver" | "basic";
  onPageChange?: (page: string) => void;
}

const navigationItems = [
  {
    title: "Dashboard",
    icon: LayoutDashboard,
    path: "dashboard",
    roles: ["manager", "supervisor", "planner", "operator"]
  },
  {
    title: "Scheduler",
    icon: Calendar,
    path: "scheduler",
    roles: ["manager", "supervisor", "planner"]
  },
  {
    title: "Machine Schedule",
    icon: CalendarClock,
    path: "machine-scheduler",
    roles: ["manager", "supervisor", "planner", "operator"]
  },
  {
    title: "Work Orders",
    icon: FileText,
    path: "work-orders",
    roles: ["manager", "supervisor", "planner", "operator"]
  },
  {
    title: "Create Work Order",
    icon: FilePlus,
    path: "work-order-create",
    roles: ["manager", "supervisor", "planner"]
  },
  {
    title: "Process Work Order",
    icon: PlayCircle,
    path: "work-order-process",
    roles: ["operator", "supervisor"]
  },
  {
    title: "Pulling",
    icon: Package,
    path: "pulling",
    roles: ["manager", "supervisor", "operator"]
  },
  {
    title: "Inventory",
    icon: Warehouse,
    path: "inventory",
    roles: ["manager", "supervisor", "planner", "operator"]
  },
  {
    title: "Loads & Shipping",
    icon: Truck,
    path: "shipping",
    roles: ["manager", "supervisor", "planner"]
  },
  {
    title: "Customers",
    icon: Users,
    path: "customers",
    roles: ["manager", "supervisor", "planner"]
  },
  {
    title: "Machines",
    icon: Settings,
    path: "machines",
    roles: ["manager", "supervisor"]
  },
  {
    title: "Admin",
    icon: Settings,
    path: "admin",
    roles: ["manager"]
  },
  {
    title: "Reports",
    icon: BarChart3,
    path: "reports",
    roles: ["manager", "supervisor"]
  }
];

export function AppLayout({ children, currentPage = "dashboard", userRole = "manager", onPageChange }: AppLayoutProps) {
  const [sidebarOpen, setSidebarOpen] = useState(true);
  
  const filteredNavItems = navigationItems.filter(item => 
    item.roles.includes(userRole)
  );

  return (
    <SidebarProvider open={sidebarOpen} onOpenChange={setSidebarOpen}>
      <div className="flex min-h-screen w-full">
        <Sidebar className="border-r bg-white">
          <SidebarHeader className="border-b p-4">
            <div className="flex items-center gap-2">
              <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-teal-600">
                <Warehouse className="h-4 w-4 text-white" />
              </div>
              <div className="grid flex-1 text-left">
                <span className="truncate font-semibold">MetalFlow WMS</span>
                <span className="truncate text-xs text-muted-foreground">
                  {userRole.charAt(0).toUpperCase() + userRole.slice(1)}
                </span>
              </div>
            </div>
          </SidebarHeader>
          <SidebarContent>
            <SidebarMenu className="p-2">
              {filteredNavItems.map((item) => (
                <SidebarMenuItem key={item.path}>
                  <SidebarMenuButton 
                    asChild 
                    isActive={currentPage === item.path}
                    className="w-full justify-start"
                  >
                    <button 
                      className="flex items-center gap-2 px-3 py-2"
                      onClick={() => onPageChange?.(item.path)}
                    >
                      <item.icon className="h-4 w-4" />
                      <span>{item.title}</span>
                    </button>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarContent>
        </Sidebar>

        <div className="flex flex-1 flex-col">
          {/* Top Navigation Bar */}
          <header className="flex h-14 items-center gap-4 border-b bg-white px-4 lg:px-6">
            <SidebarTrigger className="h-8 w-8" />
            
            <div className="flex-1 flex items-center gap-4">
              <div className="relative flex-1 max-w-md">
                <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input 
                  placeholder="Search orders, customers, products..." 
                  className="pl-9 w-full"
                />
              </div>
            </div>

            <div className="flex items-center gap-2">
              <Button variant="ghost" size="sm" className="relative">
                <Bell className="h-4 w-4" />
                <Badge className="absolute -top-1 -right-1 h-5 w-5 rounded-full p-0 text-xs bg-red-500 text-white">
                  3
                </Badge>
              </Button>
              
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="ghost" size="sm" className="gap-2">
                    <User className="h-4 w-4" />
                    <span className="hidden sm:inline">John Doe</span>
                    <ChevronDown className="h-4 w-4" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-56">
                  <DropdownMenuLabel>My Account</DropdownMenuLabel>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem>Profile</DropdownMenuItem>
                  <DropdownMenuItem>Settings</DropdownMenuItem>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem>Sign out</DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            </div>
          </header>

          {/* Main Content */}
          <main className="flex-1 overflow-y-auto bg-gray-50 p-4 lg:p-6">
            {children}
          </main>
        </div>
      </div>
    </SidebarProvider>
  );
}