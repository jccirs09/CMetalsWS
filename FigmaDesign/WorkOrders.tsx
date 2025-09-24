import { useState } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Input } from "./ui/input";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "./ui/tabs";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./ui/select";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "./ui/sheet";
import { Separator } from "./ui/separator";
import { ScrollArea } from "./ui/scroll-area";
import {
  Filter,
  Search,
  Eye,
  Play,
  Pause,
  Square,
  Clock,
  User,
  Package,
  Settings,
  AlertTriangle,
  CheckCircle,
  FilePlus,
  PlayCircle
} from "lucide-react";
import { StatusChip, Status } from "./StatusChip";

interface WorkOrdersProps {
  onNavigate?: (page: string) => void;
}

interface WorkOrder {
  id: string;
  salesOrder: string;
  customer: string;
  machine: "CTL Line 1" | "CTL Line 2" | "Slitter 1" | "Slitter 2" | "Picking" | "Packing";
  product: string;
  gauge: string;
  width: string;
  length: string;
  weight: number;
  status: Status;
  priority: "low" | "normal" | "high" | "urgent";
  plannedStart: string;
  plannedEnd: string;
  actualStart?: string;
  actualEnd?: string;
  estimatedLbsPerHour: number;
  actualLbs?: number;
  operator?: string;
  dueDate: string;
  events: Array<{
    type: "created" | "started" | "paused" | "resumed" | "completed" | "error";
    timestamp: string;
    operator: string;
    notes?: string;
  }>;
}

const workOrders: WorkOrder[] = [
  {
    id: "WO-2024001",
    salesOrder: "SO-45621",
    customer: "Industrial Metals Co",
    machine: "CTL Line 1",
    product: "Hot Rolled Coil - 16 GA x 48\" x 120\"",
    gauge: "16 GA",
    width: "48\"",
    length: "120\"",
    weight: 2400,
    status: "in-progress",
    priority: "normal",
    plannedStart: "2024-12-13 09:00",
    plannedEnd: "2024-12-13 11:30",
    actualStart: "2024-12-13 09:15",
    estimatedLbsPerHour: 1000,
    actualLbs: 1560,
    operator: "Mike Johnson",
    dueDate: "2024-12-13",
    events: [
      { type: "created", timestamp: "2024-12-12 16:30", operator: "Sarah Planning" },
      { type: "started", timestamp: "2024-12-13 09:15", operator: "Mike Johnson" },
    ]
  },
  {
    id: "WO-2024002",
    salesOrder: "SO-45622",
    customer: "Precision Parts LLC",
    machine: "Slitter 2",
    product: "Cold Rolled Sheet - 20 GA x 24\" x 96\"",
    gauge: "20 GA",
    width: "24\"",
    length: "96\"",
    weight: 1800,
    status: "in-progress",
    priority: "high",
    plannedStart: "2024-12-13 10:30",
    plannedEnd: "2024-12-13 12:15",
    actualStart: "2024-12-13 10:30",
    estimatedLbsPerHour: 900,
    actualLbs: 540,
    operator: "Sarah Chen",
    dueDate: "2024-12-13",
    events: [
      { type: "created", timestamp: "2024-12-12 14:20", operator: "Sarah Planning" },
      { type: "started", timestamp: "2024-12-13 10:30", operator: "Sarah Chen" },
    ]
  },
  {
    id: "WO-2024003",
    salesOrder: "SO-45623",
    customer: "Metro Construction",
    machine: "CTL Line 2",
    product: "Hot Rolled Coil - 14 GA x 60\" x 144\"",
    gauge: "14 GA",
    width: "60\"",
    length: "144\"",
    weight: 3200,
    status: "ready",
    priority: "normal",
    plannedStart: "2024-12-13 13:00",
    plannedEnd: "2024-12-13 16:30",
    estimatedLbsPerHour: 950,
    dueDate: "2024-12-14",
    events: [
      { type: "created", timestamp: "2024-12-12 11:45", operator: "Sarah Planning" },
    ]
  },
  {
    id: "WO-2024004",
    salesOrder: "SO-45624",
    customer: "Steel Solutions Inc",
    machine: "Picking",
    product: "Mixed Sheets - Various Gauges",
    gauge: "Various",
    width: "Various",
    length: "Various",
    weight: 2100,
    status: "planned",
    priority: "low",
    plannedStart: "2024-12-13 14:00",
    plannedEnd: "2024-12-13 16:00",
    estimatedLbsPerHour: 1050,
    dueDate: "2024-12-15",
    events: [
      { type: "created", timestamp: "2024-12-12 10:30", operator: "Sarah Planning" },
    ]
  },
];

const priorityColors = {
  low: "bg-gray-100 text-gray-700",
  normal: "bg-blue-100 text-blue-700",
  high: "bg-orange-100 text-orange-700",
  urgent: "bg-red-100 text-red-700"
};

export function WorkOrders({ onNavigate }: WorkOrdersProps = {}) {
  const [selectedStatus, setSelectedStatus] = useState<string>("all");
  const [selectedMachine, setSelectedMachine] = useState<string>("all");
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedWorkOrder, setSelectedWorkOrder] = useState<WorkOrder | null>(null);

  const filteredOrders = workOrders.filter(order => {
    const matchesStatus = selectedStatus === "all" || order.status === selectedStatus;
    const matchesMachine = selectedMachine === "all" || order.machine === selectedMachine;
    const matchesSearch = searchQuery === "" || 
      order.id.toLowerCase().includes(searchQuery.toLowerCase()) ||
      order.customer.toLowerCase().includes(searchQuery.toLowerCase()) ||
      order.salesOrder.toLowerCase().includes(searchQuery.toLowerCase());
    
    return matchesStatus && matchesMachine && matchesSearch;
  });

  const statusCounts = {
    all: workOrders.length,
    planned: workOrders.filter(o => o.status === "planned").length,
    ready: workOrders.filter(o => o.status === "ready").length,
    "in-progress": workOrders.filter(o => o.status === "in-progress").length,
    paused: workOrders.filter(o => o.status === "paused").length,
    completed: workOrders.filter(o => o.status === "completed").length,
  };

  const KanbanColumn = ({ status, title, orders }: { status: string; title: string; orders: WorkOrder[] }) => (
    <div className="flex-1 min-w-80">
      <div className="bg-gray-50 rounded-lg p-4">
        <div className="flex items-center justify-between mb-4">
          <h3 className="font-medium">{title}</h3>
          <Badge variant="secondary">{orders.length}</Badge>
        </div>
        <div className="space-y-3">
          {orders.map(order => (
            <Card key={order.id} className="p-3 cursor-pointer hover:shadow-md transition-shadow">
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <Badge variant="outline" className="text-xs">{order.id}</Badge>
                  <Badge className={priorityColors[order.priority]}>{order.priority}</Badge>
                </div>
                <p className="font-medium text-sm">{order.customer}</p>
                <p className="text-xs text-muted-foreground">{order.product}</p>
                <div className="flex items-center justify-between text-xs">
                  <span className="flex items-center gap-1">
                    <Settings className="h-3 w-3" />
                    {order.machine}
                  </span>
                  <span className="flex items-center gap-1">
                    <Package className="h-3 w-3" />
                    {order.weight.toLocaleString()} lbs
                  </span>
                </div>
                {order.operator && (
                  <div className="flex items-center gap-1 text-xs text-muted-foreground">
                    <User className="h-3 w-3" />
                    {order.operator}
                  </div>
                )}
                <div className="flex items-center justify-between pt-2">
                  <span className="text-xs text-muted-foreground">Due: {order.dueDate}</span>
                  <Sheet>
                    <SheetTrigger asChild>
                      <Button 
                        variant="ghost" 
                        size="sm" 
                        className="h-6 px-2"
                        onClick={() => setSelectedWorkOrder(order)}
                      >
                        <Eye className="h-3 w-3" />
                      </Button>
                    </SheetTrigger>
                  </Sheet>
                </div>
              </div>
            </Card>
          ))}
        </div>
      </div>
    </div>
  );

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1>Work Orders</h1>
          <p className="text-muted-foreground">
            Manage production work orders across all machines
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Button 
            variant="outline"
            onClick={() => onNavigate?.("work-order-process")}
          >
            <PlayCircle className="h-4 w-4 mr-2" />
            Process Orders
          </Button>
          <Button 
            className="bg-teal-600 hover:bg-teal-700"
            onClick={() => onNavigate?.("work-order-create")}
          >
            <FilePlus className="h-4 w-4 mr-2" />
            Create Work Order
          </Button>
        </div>
      </div>

      <Tabs defaultValue="kanban" className="space-y-4">
        <TabsList>
          <TabsTrigger value="kanban">Kanban View</TabsTrigger>
          <TabsTrigger value="list">List View</TabsTrigger>
        </TabsList>

        {/* Filters */}
        <div className="flex items-center gap-4">
          <div className="relative flex-1 max-w-sm">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Search work orders..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-9"
            />
          </div>
          <Select value={selectedStatus} onValueChange={setSelectedStatus}>
            <SelectTrigger className="w-48">
              <SelectValue placeholder="Filter by status" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Status ({statusCounts.all})</SelectItem>
              <SelectItem value="planned">Planned ({statusCounts.planned})</SelectItem>
              <SelectItem value="ready">Ready ({statusCounts.ready})</SelectItem>
              <SelectItem value="in-progress">In Progress ({statusCounts["in-progress"]})</SelectItem>
              <SelectItem value="paused">Paused ({statusCounts.paused})</SelectItem>
              <SelectItem value="completed">Completed ({statusCounts.completed})</SelectItem>
            </SelectContent>
          </Select>
          <Select value={selectedMachine} onValueChange={setSelectedMachine}>
            <SelectTrigger className="w-48">
              <SelectValue placeholder="Filter by machine" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Machines</SelectItem>
              <SelectItem value="CTL Line 1">CTL Line 1</SelectItem>
              <SelectItem value="CTL Line 2">CTL Line 2</SelectItem>
              <SelectItem value="Slitter 1">Slitter 1</SelectItem>
              <SelectItem value="Slitter 2">Slitter 2</SelectItem>
              <SelectItem value="Picking">Picking</SelectItem>
              <SelectItem value="Packing">Packing</SelectItem>
            </SelectContent>
          </Select>
        </div>

        <TabsContent value="kanban" className="space-y-4">
          <div className="flex gap-4 overflow-x-auto pb-4">
            <KanbanColumn 
              status="planned" 
              title="Planned" 
              orders={filteredOrders.filter(o => o.status === "planned")} 
            />
            <KanbanColumn 
              status="ready" 
              title="Ready" 
              orders={filteredOrders.filter(o => o.status === "ready")} 
            />
            <KanbanColumn 
              status="in-progress" 
              title="In Progress" 
              orders={filteredOrders.filter(o => o.status === "in-progress")} 
            />
            <KanbanColumn 
              status="paused" 
              title="Paused" 
              orders={filteredOrders.filter(o => o.status === "paused")} 
            />
            <KanbanColumn 
              status="completed" 
              title="Completed" 
              orders={filteredOrders.filter(o => o.status === "completed")} 
            />
          </div>
        </TabsContent>

        <TabsContent value="list" className="space-y-4">
          <div className="grid gap-4">
            {filteredOrders.map(order => (
              <Card key={order.id}>
                <CardContent className="p-4">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-4">
                      <div>
                        <div className="flex items-center gap-2 mb-1">
                          <Badge variant="outline">{order.id}</Badge>
                          <StatusChip status={order.status} />
                          <Badge className={priorityColors[order.priority]}>{order.priority}</Badge>
                        </div>
                        <h3 className="font-medium">{order.customer}</h3>
                        <p className="text-sm text-muted-foreground">{order.product}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-4 text-sm">
                      <div className="text-right">
                        <p className="text-muted-foreground">Machine</p>
                        <p className="font-medium">{order.machine}</p>
                      </div>
                      <div className="text-right">
                        <p className="text-muted-foreground">Weight</p>
                        <p className="font-medium">{order.weight.toLocaleString()} lbs</p>
                      </div>
                      <div className="text-right">
                        <p className="text-muted-foreground">Due Date</p>
                        <p className="font-medium">{order.dueDate}</p>
                      </div>
                      <div className="flex gap-2">
                        {order.status === "ready" && (
                          <Button size="sm" className="bg-teal-600 hover:bg-teal-700">
                            <Play className="h-4 w-4 mr-1" />
                            Start
                          </Button>
                        )}
                        {order.status === "in-progress" && (
                          <Button size="sm" variant="outline">
                            <Pause className="h-4 w-4 mr-1" />
                            Pause
                          </Button>
                        )}
                        <Button size="sm" variant="ghost">
                          <Eye className="h-4 w-4" />
                        </Button>
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>
      </Tabs>

      {/* Work Order Detail Sheet */}
      <Sheet open={selectedWorkOrder !== null} onOpenChange={() => setSelectedWorkOrder(null)}>
        <SheetContent className="w-[600px] sm:max-w-[600px]">
          {selectedWorkOrder && (
            <>
              <SheetHeader>
                <SheetTitle className="flex items-center gap-2">
                  {selectedWorkOrder.id}
                  <StatusChip status={selectedWorkOrder.status} />
                </SheetTitle>
                <SheetDescription>
                  Work order details and event history
                </SheetDescription>
              </SheetHeader>
              
              <ScrollArea className="h-[calc(100vh-120px)] mt-6">
                <div className="space-y-6">
                  {/* Order Info */}
                  <div className="space-y-4">
                    <h3>Order Information</h3>
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <p className="text-muted-foreground">Sales Order</p>
                        <p className="font-medium">{selectedWorkOrder.salesOrder}</p>
                      </div>
                      <div>
                        <p className="text-muted-foreground">Customer</p>
                        <p className="font-medium">{selectedWorkOrder.customer}</p>
                      </div>
                      <div>
                        <p className="text-muted-foreground">Machine</p>
                        <p className="font-medium">{selectedWorkOrder.machine}</p>
                      </div>
                      <div>
                        <p className="text-muted-foreground">Priority</p>
                        <Badge className={priorityColors[selectedWorkOrder.priority]}>
                          {selectedWorkOrder.priority}
                        </Badge>
                      </div>
                    </div>
                  </div>

                  <Separator />

                  {/* Product Info */}
                  <div className="space-y-4">
                    <h3>Product Details</h3>
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <p className="text-muted-foreground">Gauge</p>
                        <p className="font-medium">{selectedWorkOrder.gauge}</p>
                      </div>
                      <div>
                        <p className="text-muted-foreground">Width</p>
                        <p className="font-medium">{selectedWorkOrder.width}</p>
                      </div>
                      <div>
                        <p className="text-muted-foreground">Length</p>
                        <p className="font-medium">{selectedWorkOrder.length}</p>
                      </div>
                      <div>
                        <p className="text-muted-foreground">Weight</p>
                        <p className="font-medium">{selectedWorkOrder.weight.toLocaleString()} lbs</p>
                      </div>
                    </div>
                  </div>

                  <Separator />

                  {/* Timeline */}
                  <div className="space-y-4">
                    <h3>Schedule & Progress</h3>
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <p className="text-muted-foreground">Planned Start</p>
                        <p className="font-medium">{new Date(selectedWorkOrder.plannedStart).toLocaleString()}</p>
                      </div>
                      <div>
                        <p className="text-muted-foreground">Planned End</p>
                        <p className="font-medium">{new Date(selectedWorkOrder.plannedEnd).toLocaleString()}</p>
                      </div>
                      {selectedWorkOrder.actualStart && (
                        <div>
                          <p className="text-muted-foreground">Actual Start</p>
                          <p className="font-medium">{new Date(selectedWorkOrder.actualStart).toLocaleString()}</p>
                        </div>
                      )}
                      <div>
                        <p className="text-muted-foreground">Due Date</p>
                        <p className="font-medium">{selectedWorkOrder.dueDate}</p>
                      </div>
                    </div>
                  </div>

                  <Separator />

                  {/* Event History */}
                  <div className="space-y-4">
                    <h3>Event History</h3>
                    <div className="space-y-3">
                      {selectedWorkOrder.events.map((event, index) => (
                        <div key={index} className="flex items-start gap-3 p-3 bg-gray-50 rounded-lg">
                          <div className="flex h-6 w-6 items-center justify-center rounded-full bg-teal-100">
                            {event.type === "created" && <Clock className="h-3 w-3 text-teal-600" />}
                            {event.type === "started" && <Play className="h-3 w-3 text-teal-600" />}
                            {event.type === "paused" && <Pause className="h-3 w-3 text-amber-600" />}
                            {event.type === "completed" && <CheckCircle className="h-3 w-3 text-green-600" />}
                            {event.type === "error" && <AlertTriangle className="h-3 w-3 text-red-600" />}
                          </div>
                          <div className="flex-1 text-sm">
                            <p className="font-medium capitalize">{event.type.replace("-", " ")}</p>
                            <p className="text-muted-foreground">
                              {new Date(event.timestamp).toLocaleString()} • {event.operator}
                            </p>
                            {event.notes && (
                              <p className="mt-1 text-muted-foreground">{event.notes}</p>
                            )}
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                </div>
              </ScrollArea>
            </>
          )}
        </SheetContent>
      </Sheet>
    </div>
  );
}