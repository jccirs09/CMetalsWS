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
  PlayCircle,
  FileText,
  MapPin,
  Building2
} from "lucide-react";

interface WorkOrdersProps {
  onNavigate?: (page: string) => void;
}

// Enums matching C# entities
enum WorkOrderStatus {
  Draft = "Draft",
  Pending = "Pending", 
  InProgress = "InProgress",
  Completed = "Completed",
  Canceled = "Canceled",
  Awaiting = "Awaiting"
}

enum MachineCategory {
  CTL = "CTL",
  Slitter = "Slitter", 
  Picking = "Picking",
  Packing = "Packing",
  Crane = "Crane"
}

// Interfaces matching C# entities
interface Branch {
  id: number;
  name: string;
  code: string;
}

interface Machine {
  id: number;
  name: string;
  category: MachineCategory;
  isActive: boolean;
}

interface WorkOrderItem {
  id: number;
  workOrderId: number;
  pickingListItemId?: number;
  itemCode: string;
  description: string;
  salesOrderNumber?: string;
  customerName?: string;
  orderQuantity?: number;
  orderWeight?: number;
  width?: number;
  length?: number;
  producedQuantity?: number;
  producedWeight?: number;
  unit?: string;
  location?: string;
  isStockItem: boolean;
}

interface WorkOrder {
  id: number;
  workOrderNumber: string;
  pdfWorkOrderNumber?: string;
  tagNumber: string;
  branchId: number;
  branch?: Branch;
  machineId?: number;
  machine?: Machine;
  machineCategory: MachineCategory;
  dueDate: string;
  parentItemId?: string;
  instructions?: string;
  createdBy?: string;
  createdDate: string;
  lastUpdatedBy?: string;
  lastUpdatedDate: string;
  scheduledStartDate: string;
  scheduledEndDate: string;
  status: WorkOrderStatus;
  shift?: string;
  items: WorkOrderItem[];
  // Additional fields for tracking
  priority: "low" | "normal" | "high" | "urgent";
  actualStartDate?: string;
  actualEndDate?: string;
  operator?: string;
  events: Array<{
    type: "created" | "started" | "paused" | "resumed" | "completed" | "error";
    timestamp: string;
    operator: string;
    notes?: string;
  }>;
}

// Mock data matching new entity structure
const branches: Branch[] = [
  { id: 1, name: "Main Facility", code: "MAIN" },
  { id: 2, name: "East Warehouse", code: "EAST" }
];

const machines: Machine[] = [
  { id: 1, name: "CTL Line 1", category: MachineCategory.CTL, isActive: true },
  { id: 2, name: "CTL Line 2", category: MachineCategory.CTL, isActive: true },
  { id: 3, name: "Slitter 1", category: MachineCategory.Slitter, isActive: true },
  { id: 4, name: "Slitter 2", category: MachineCategory.Slitter, isActive: true },
  { id: 5, name: "Picking Line 1", category: MachineCategory.Picking, isActive: true },
  { id: 6, name: "Packing Station 1", category: MachineCategory.Packing, isActive: true }
];

const workOrders: WorkOrder[] = [
  {
    id: 1,
    workOrderNumber: "WO-2024001",
    pdfWorkOrderNumber: "PDF-WO-2024001",
    tagNumber: "TAG-001",
    branchId: 1,
    branch: branches[0],
    machineId: 1,
    machine: machines[0],
    machineCategory: MachineCategory.CTL,
    dueDate: "2024-12-13",
    parentItemId: "COIL-HR-16-48-120",
    instructions: "Cut to length according to customer specifications. Ensure clean edges.",
    createdBy: "Sarah Planning",
    createdDate: "2024-12-12T16:30:00Z",
    lastUpdatedBy: "Mike Johnson",
    lastUpdatedDate: "2024-12-13T09:15:00Z",
    scheduledStartDate: "2024-12-13T09:00:00Z",
    scheduledEndDate: "2024-12-13T11:30:00Z",
    status: WorkOrderStatus.InProgress,
    shift: "Day",
    priority: "normal",
    actualStartDate: "2024-12-13T09:15:00Z",
    operator: "Mike Johnson",
    items: [
      {
        id: 1,
        workOrderId: 1,
        itemCode: "HR-16-48-120",
        description: "Hot Rolled Coil - 16 GA x 48\" x 120\"",
        salesOrderNumber: "SO-45621",
        customerName: "Industrial Metals Co",
        orderQuantity: 1,
        orderWeight: 2400,
        width: 48,
        length: 120,
        producedQuantity: 0.65,
        producedWeight: 1560,
        unit: "PCS",
        location: "A-12-03",
        isStockItem: false
      }
    ],
    events: [
      { type: "created", timestamp: "2024-12-12T16:30:00Z", operator: "Sarah Planning" },
      { type: "started", timestamp: "2024-12-13T09:15:00Z", operator: "Mike Johnson" }
    ]
  },
  {
    id: 2,
    workOrderNumber: "WO-2024002",
    pdfWorkOrderNumber: "PDF-WO-2024002",
    tagNumber: "TAG-002",
    branchId: 1,
    branch: branches[0],
    machineId: 4,
    machine: machines[3],
    machineCategory: MachineCategory.Slitter,
    dueDate: "2024-12-13",
    parentItemId: "SHEET-CR-20-24-96",
    instructions: "Slit to 24\" width, maintain tight tolerances.",
    createdBy: "Sarah Planning",
    createdDate: "2024-12-12T14:20:00Z",
    lastUpdatedBy: "Sarah Chen",
    lastUpdatedDate: "2024-12-13T10:30:00Z",
    scheduledStartDate: "2024-12-13T10:30:00Z",
    scheduledEndDate: "2024-12-13T12:15:00Z",
    status: WorkOrderStatus.InProgress,
    shift: "Day",
    priority: "high",
    actualStartDate: "2024-12-13T10:30:00Z",
    operator: "Sarah Chen",
    items: [
      {
        id: 2,
        workOrderId: 2,
        itemCode: "CR-20-24-96",
        description: "Cold Rolled Sheet - 20 GA x 24\" x 96\"",
        salesOrderNumber: "SO-45622",
        customerName: "Precision Parts LLC",
        orderQuantity: 5,
        orderWeight: 1800,
        width: 24,
        length: 96,
        producedQuantity: 3,
        producedWeight: 540,
        unit: "PCS",
        location: "B-05-12",
        isStockItem: false
      }
    ],
    events: [
      { type: "created", timestamp: "2024-12-12T14:20:00Z", operator: "Sarah Planning" },
      { type: "started", timestamp: "2024-12-13T10:30:00Z", operator: "Sarah Chen" }
    ]
  },
  {
    id: 3,
    workOrderNumber: "WO-2024003",
    pdfWorkOrderNumber: "PDF-WO-2024003",
    tagNumber: "TAG-003",
    branchId: 1,
    branch: branches[0],
    machineId: 2,
    machine: machines[1],
    machineCategory: MachineCategory.CTL,
    dueDate: "2024-12-14",
    parentItemId: "COIL-HR-14-60-144",
    instructions: "Large coil processing - coordinate with crane operator.",
    createdBy: "Sarah Planning",
    createdDate: "2024-12-12T11:45:00Z",
    lastUpdatedBy: "Sarah Planning",
    lastUpdatedDate: "2024-12-12T11:45:00Z",
    scheduledStartDate: "2024-12-13T13:00:00Z",
    scheduledEndDate: "2024-12-13T16:30:00Z",
    status: WorkOrderStatus.Pending,
    shift: "Day",
    priority: "normal",
    items: [
      {
        id: 3,
        workOrderId: 3,
        itemCode: "HR-14-60-144",
        description: "Hot Rolled Coil - 14 GA x 60\" x 144\"",
        salesOrderNumber: "SO-45623",
        customerName: "Metro Construction",
        orderQuantity: 1,
        orderWeight: 3200,
        width: 60,
        length: 144,
        unit: "PCS",
        location: "A-15-01",
        isStockItem: false
      }
    ],
    events: [
      { type: "created", timestamp: "2024-12-12T11:45:00Z", operator: "Sarah Planning" }
    ]
  },
  {
    id: 4,
    workOrderNumber: "WO-2024004",
    pdfWorkOrderNumber: "PDF-WO-2024004", 
    tagNumber: "TAG-004",
    branchId: 1,
    branch: branches[0],
    machineId: 5,
    machine: machines[4],
    machineCategory: MachineCategory.Picking,
    dueDate: "2024-12-15",
    instructions: "Multiple items for customer pickup - group by destination.",
    createdBy: "Sarah Planning",
    createdDate: "2024-12-12T10:30:00Z",
    lastUpdatedBy: "Sarah Planning",
    lastUpdatedDate: "2024-12-12T10:30:00Z",
    scheduledStartDate: "2024-12-13T14:00:00Z",
    scheduledEndDate: "2024-12-13T16:00:00Z",
    status: WorkOrderStatus.Draft,
    shift: "Day",
    priority: "low",
    items: [
      {
        id: 4,
        workOrderId: 4,
        itemCode: "MIXED-001",
        description: "Galvanized Sheet - 18 GA x 36\" x 72\"",
        salesOrderNumber: "SO-45624",
        customerName: "Steel Solutions Inc",
        orderQuantity: 3,
        orderWeight: 890,
        width: 36,
        length: 72,
        unit: "PCS",
        location: "C-08-15",
        isStockItem: true
      },
      {
        id: 5,
        workOrderId: 4,
        itemCode: "MIXED-002",
        description: "Aluminum Sheet - 16 GA x 48\" x 96\"",
        salesOrderNumber: "SO-45624",
        customerName: "Steel Solutions Inc",
        orderQuantity: 2,
        orderWeight: 1210,
        width: 48,
        length: 96,
        unit: "PCS",
        location: "D-03-22",
        isStockItem: true
      }
    ],
    events: [
      { type: "created", timestamp: "2024-12-12T10:30:00Z", operator: "Sarah Planning" }
    ]
  }
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

  const getStatusDisplayName = (status: WorkOrderStatus): string => {
    switch (status) {
      case WorkOrderStatus.Draft: return "draft";
      case WorkOrderStatus.Pending: return "ready";
      case WorkOrderStatus.InProgress: return "in-progress";
      case WorkOrderStatus.Completed: return "completed";
      case WorkOrderStatus.Canceled: return "error";
      case WorkOrderStatus.Awaiting: return "paused";
      default: return "planned";
    }
  };

  const filteredOrders = workOrders.filter(order => {
    const statusDisplay = getStatusDisplayName(order.status);
    const matchesStatus = selectedStatus === "all" || statusDisplay === selectedStatus;
    const matchesMachine = selectedMachine === "all" || order.machine?.name === selectedMachine;
    const matchesSearch = searchQuery === "" || 
      order.workOrderNumber.toLowerCase().includes(searchQuery.toLowerCase()) ||
      order.items.some(item => item.customerName?.toLowerCase().includes(searchQuery.toLowerCase())) ||
      order.items.some(item => item.salesOrderNumber?.toLowerCase().includes(searchQuery.toLowerCase()));
    
    return matchesStatus && matchesMachine && matchesSearch;
  });

  const statusCounts = {
    all: workOrders.length,
    draft: workOrders.filter(o => o.status === WorkOrderStatus.Draft).length,
    ready: workOrders.filter(o => o.status === WorkOrderStatus.Pending).length,
    "in-progress": workOrders.filter(o => o.status === WorkOrderStatus.InProgress).length,
    paused: workOrders.filter(o => o.status === WorkOrderStatus.Awaiting).length,
    completed: workOrders.filter(o => o.status === WorkOrderStatus.Completed).length,
    error: workOrders.filter(o => o.status === WorkOrderStatus.Canceled).length,
  };

  const getTotalWeight = (order: WorkOrder): number => {
    return order.items.reduce((total, item) => total + (item.orderWeight || 0), 0);
  };

  const getMainCustomer = (order: WorkOrder): string => {
    return order.items[0]?.customerName || "Unknown Customer";
  };

  const getMainItemDescription = (order: WorkOrder): string => {
    if (order.items.length === 1) {
      return order.items[0].description;
    }
    return `${order.items.length} items`;
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
                  <Badge variant="outline" className="text-xs">{order.workOrderNumber}</Badge>
                  <Badge className={priorityColors[order.priority]}>{order.priority}</Badge>
                </div>
                <div className="flex items-center gap-2 mb-1">
                  <Badge variant="outline" className="text-xs">{order.tagNumber}</Badge>
                  {order.pdfWorkOrderNumber && (
                    <FileText className="h-3 w-3 text-muted-foreground" />
                  )}
                </div>
                <p className="font-medium text-sm">{getMainCustomer(order)}</p>
                <p className="text-xs text-muted-foreground">{getMainItemDescription(order)}</p>
                <div className="flex items-center justify-between text-xs">
                  <span className="flex items-center gap-1">
                    <Settings className="h-3 w-3" />
                    {order.machine?.name || order.machineCategory}
                  </span>
                  <span className="flex items-center gap-1">
                    <Package className="h-3 w-3" />
                    {getTotalWeight(order).toLocaleString()} lbs
                  </span>
                </div>
                <div className="flex items-center justify-between text-xs">
                  <span className="flex items-center gap-1">
                    <Building2 className="h-3 w-3" />
                    {order.branch?.code}
                  </span>
                  {order.shift && (
                    <span className="text-muted-foreground">{order.shift} Shift</span>
                  )}
                </div>
                {order.operator && (
                  <div className="flex items-center gap-1 text-xs text-muted-foreground">
                    <User className="h-3 w-3" />
                    {order.operator}
                  </div>
                )}
                <div className="flex items-center justify-between pt-2">
                  <span className="text-xs text-muted-foreground">Due: {new Date(order.dueDate).toLocaleDateString()}</span>
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
              <SelectItem value="draft">Draft ({statusCounts.draft})</SelectItem>
              <SelectItem value="ready">Ready ({statusCounts.ready})</SelectItem>
              <SelectItem value="in-progress">In Progress ({statusCounts["in-progress"]})</SelectItem>
              <SelectItem value="paused">Awaiting ({statusCounts.paused})</SelectItem>
              <SelectItem value="completed">Completed ({statusCounts.completed})</SelectItem>
              <SelectItem value="error">Canceled ({statusCounts.error})</SelectItem>
            </SelectContent>
          </Select>
          <Select value={selectedMachine} onValueChange={setSelectedMachine}>
            <SelectTrigger className="w-48">
              <SelectValue placeholder="Filter by machine" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Machines</SelectItem>
              {machines.filter(m => m.isActive).map(machine => (
                <SelectItem key={machine.id} value={machine.name}>
                  {machine.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <TabsContent value="kanban" className="space-y-4">
          <div className="flex gap-4 overflow-x-auto pb-4">
            <KanbanColumn 
              status="draft" 
              title="Draft" 
              orders={filteredOrders.filter(o => getStatusDisplayName(o.status) === "draft")} 
            />
            <KanbanColumn 
              status="ready" 
              title="Ready" 
              orders={filteredOrders.filter(o => getStatusDisplayName(o.status) === "ready")} 
            />
            <KanbanColumn 
              status="in-progress" 
              title="In Progress" 
              orders={filteredOrders.filter(o => getStatusDisplayName(o.status) === "in-progress")} 
            />
            <KanbanColumn 
              status="paused" 
              title="Awaiting" 
              orders={filteredOrders.filter(o => getStatusDisplayName(o.status) === "paused")} 
            />
            <KanbanColumn 
              status="completed" 
              title="Completed" 
              orders={filteredOrders.filter(o => getStatusDisplayName(o.status) === "completed")} 
            />
            <KanbanColumn 
              status="error" 
              title="Canceled" 
              orders={filteredOrders.filter(o => getStatusDisplayName(o.status) === "error")} 
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
                          <Badge variant="outline">{order.workOrderNumber}</Badge>
                          <Badge variant="outline" className="text-xs">{order.tagNumber}</Badge>
                          <div className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${
                            getStatusDisplayName(order.status) === 'draft' ? 'bg-gray-100 text-gray-700' :
                            getStatusDisplayName(order.status) === 'ready' ? 'bg-blue-100 text-blue-700' :
                            getStatusDisplayName(order.status) === 'in-progress' ? 'bg-green-100 text-green-700' :
                            getStatusDisplayName(order.status) === 'paused' ? 'bg-yellow-100 text-yellow-700' :
                            getStatusDisplayName(order.status) === 'completed' ? 'bg-teal-100 text-teal-700' :
                            'bg-red-100 text-red-700'
                          }`}>
                            {order.status}
                          </div>
                          <Badge className={priorityColors[order.priority]}>{order.priority}</Badge>
                        </div>
                        <h3 className="font-medium">{getMainCustomer(order)}</h3>
                        <p className="text-sm text-muted-foreground">{getMainItemDescription(order)}</p>
                        <div className="flex items-center gap-4 text-xs text-muted-foreground mt-1">
                          <span className="flex items-center gap-1">
                            <Building2 className="h-3 w-3" />
                            {order.branch?.name}
                          </span>
                          {order.shift && (
                            <span>{order.shift} Shift</span>
                          )}
                          {order.createdBy && (
                            <span>Created by {order.createdBy}</span>
                          )}
                        </div>
                      </div>
                    </div>
                    <div className="flex items-center gap-4 text-sm">
                      <div className="text-right">
                        <p className="text-muted-foreground">Machine</p>
                        <p className="font-medium">{order.machine?.name || order.machineCategory}</p>
                      </div>
                      <div className="text-right">
                        <p className="text-muted-foreground">Total Weight</p>
                        <p className="font-medium">{getTotalWeight(order).toLocaleString()} lbs</p>
                      </div>
                      <div className="text-right">
                        <p className="text-muted-foreground">Items</p>
                        <p className="font-medium">{order.items.length}</p>
                      </div>
                      <div className="text-right">
                        <p className="text-muted-foreground">Due Date</p>
                        <p className="font-medium">{new Date(order.dueDate).toLocaleDateString()}</p>
                      </div>
                      <div className="flex gap-2">
                        {getStatusDisplayName(order.status) === "ready" && (
                          <Button size="sm" className="bg-teal-600 hover:bg-teal-700">
                            <Play className="h-4 w-4 mr-1" />
                            Start
                          </Button>
                        )}
                        {getStatusDisplayName(order.status) === "in-progress" && (
                          <Button size="sm" variant="outline">
                            <Pause className="h-4 w-4 mr-1" />
                            Pause
                          </Button>
                        )}
                        <Button 
                          size="sm" 
                          variant="ghost"
                          onClick={() => setSelectedWorkOrder(order)}
                        >
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
        <SheetContent className="w-[700px] sm:max-w-[700px]">
          {selectedWorkOrder && (
            <>
              <SheetHeader>
                <SheetTitle className="flex items-center gap-2">
                  {selectedWorkOrder.workOrderNumber}
                  <div className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${
                    getStatusDisplayName(selectedWorkOrder.status) === 'draft' ? 'bg-gray-100 text-gray-700' :
                    getStatusDisplayName(selectedWorkOrder.status) === 'ready' ? 'bg-blue-100 text-blue-700' :
                    getStatusDisplayName(selectedWorkOrder.status) === 'in-progress' ? 'bg-green-100 text-green-700' :
                    getStatusDisplayName(selectedWorkOrder.status) === 'paused' ? 'bg-yellow-100 text-yellow-700' :
                    getStatusDisplayName(selectedWorkOrder.status) === 'completed' ? 'bg-teal-100 text-teal-700' :
                    'bg-red-100 text-red-700'
                  }`}>
                    {selectedWorkOrder.status}
                  </div>
                </SheetTitle>
                <SheetDescription>
                  Work order details, items, and event history
                </SheetDescription>
              </SheetHeader>
              
              <ScrollArea className="h-[calc(100vh-120px)] mt-6">
                <div className="space-y-6">
                  {/* Order Info */}
                  <div className="space-y-4">
                    <h3>Order Information</h3>
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <p className="text-muted-foreground">Work Order Number</p>
                        <p className="font-medium">{selectedWorkOrder.workOrderNumber}</p>
                      </div>
                      <div>
                        <p className="text-muted-foreground">Tag Number</p>
                        <p className="font-medium">{selectedWorkOrder.tagNumber}</p>
                      </div>
                      {selectedWorkOrder.pdfWorkOrderNumber && (
                        <div>
                          <p className="text-muted-foreground">PDF Work Order</p>
                          <p className="font-medium flex items-center gap-1">
                            <FileText className="h-3 w-3" />
                            {selectedWorkOrder.pdfWorkOrderNumber}
                          </p>
                        </div>
                      )}
                      <div>
                        <p className="text-muted-foreground">Branch</p>
                        <p className="font-medium">{selectedWorkOrder.branch?.name} ({selectedWorkOrder.branch?.code})</p>
                      </div>
                      <div>
                        <p className="text-muted-foreground">Machine</p>
                        <p className="font-medium">{selectedWorkOrder.machine?.name || selectedWorkOrder.machineCategory}</p>
                      </div>
                      <div>
                        <p className="text-muted-foreground">Priority</p>
                        <Badge className={priorityColors[selectedWorkOrder.priority]}>
                          {selectedWorkOrder.priority}
                        </Badge>
                      </div>
                      {selectedWorkOrder.shift && (
                        <div>
                          <p className="text-muted-foreground">Shift</p>
                          <p className="font-medium">{selectedWorkOrder.shift}</p>
                        </div>
                      )}
                      {selectedWorkOrder.operator && (
                        <div>
                          <p className="text-muted-foreground">Operator</p>
                          <p className="font-medium">{selectedWorkOrder.operator}</p>
                        </div>
                      )}
                    </div>
                    {selectedWorkOrder.instructions && (
                      <div>
                        <p className="text-muted-foreground">Instructions</p>
                        <p className="text-sm bg-gray-50 p-3 rounded-lg mt-1">{selectedWorkOrder.instructions}</p>
                      </div>
                    )}
                  </div>

                  <Separator />

                  {/* Work Order Items */}
                  <div className="space-y-4">
                    <h3>Items ({selectedWorkOrder.items.length})</h3>
                    <div className="space-y-3">
                      {selectedWorkOrder.items.map((item, index) => (
                        <Card key={item.id} className="p-3">
                          <div className="space-y-3">
                            <div className="flex items-start justify-between">
                              <div>
                                <div className="flex items-center gap-2 mb-1">
                                  <Badge variant="outline" className="text-xs">{item.itemCode}</Badge>
                                  {item.isStockItem && (
                                    <Badge variant="secondary" className="text-xs">Stock Item</Badge>
                                  )}
                                </div>
                                <p className="font-medium text-sm">{item.description}</p>
                                {item.customerName && (
                                  <p className="text-xs text-muted-foreground">{item.customerName}</p>
                                )}
                              </div>
                              {item.location && (
                                <div className="flex items-center gap-1 text-xs text-muted-foreground">
                                  <MapPin className="h-3 w-3" />
                                  {item.location}
                                </div>
                              )}
                            </div>
                            
                            <div className="grid grid-cols-3 gap-4 text-xs">
                              {item.salesOrderNumber && (
                                <div>
                                  <p className="text-muted-foreground">Sales Order</p>
                                  <p className="font-medium">{item.salesOrderNumber}</p>
                                </div>
                              )}
                              {item.orderQuantity && (
                                <div>
                                  <p className="text-muted-foreground">Order Qty</p>
                                  <p className="font-medium">{item.orderQuantity} {item.unit}</p>
                                </div>
                              )}
                              {item.orderWeight && (
                                <div>
                                  <p className="text-muted-foreground">Order Weight</p>
                                  <p className="font-medium">{item.orderWeight.toLocaleString()} lbs</p>
                                </div>
                              )}
                              {item.width && (
                                <div>
                                  <p className="text-muted-foreground">Width</p>
                                  <p className="font-medium">{item.width}\"</p>
                                </div>
                              )}
                              {item.length && (
                                <div>
                                  <p className="text-muted-foreground">Length</p>
                                  <p className="font-medium">{item.length}\"</p>
                                </div>
                              )}
                            </div>

                            {(item.producedQuantity || item.producedWeight) && (
                              <>
                                <Separator />
                                <div className="grid grid-cols-2 gap-4 text-xs">
                                  {item.producedQuantity && (
                                    <div>
                                      <p className="text-muted-foreground">Produced Qty</p>
                                      <p className="font-medium text-green-600">{item.producedQuantity} {item.unit}</p>
                                    </div>
                                  )}
                                  {item.producedWeight && (
                                    <div>
                                      <p className="text-muted-foreground">Produced Weight</p>
                                      <p className="font-medium text-green-600">{item.producedWeight.toLocaleString()} lbs</p>
                                    </div>
                                  )}
                                </div>
                              </>
                            )}
                          </div>
                        </Card>
                      ))}
                    </div>
                  </div>

                  <Separator />

                  {/* Timeline */}
                  <div className="space-y-4">
                    <h3>Schedule & Progress</h3>
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <p className="text-muted-foreground">Scheduled Start</p>
                        <p className="font-medium">{new Date(selectedWorkOrder.scheduledStartDate).toLocaleString()}</p>
                      </div>
                      <div>
                        <p className="text-muted-foreground">Scheduled End</p>
                        <p className="font-medium">{new Date(selectedWorkOrder.scheduledEndDate).toLocaleString()}</p>
                      </div>
                      {selectedWorkOrder.actualStartDate && (
                        <div>
                          <p className="text-muted-foreground">Actual Start</p>
                          <p className="font-medium">{new Date(selectedWorkOrder.actualStartDate).toLocaleString()}</p>
                        </div>
                      )}
                      {selectedWorkOrder.actualEndDate && (
                        <div>
                          <p className="text-muted-foreground">Actual End</p>
                          <p className="font-medium">{new Date(selectedWorkOrder.actualEndDate).toLocaleString()}</p>
                        </div>
                      )}
                      <div>
                        <p className="text-muted-foreground">Due Date</p>
                        <p className="font-medium">{new Date(selectedWorkOrder.dueDate).toLocaleDateString()}</p>
                      </div>
                      <div>
                        <p className="text-muted-foreground">Created</p>
                        <p className="font-medium">{new Date(selectedWorkOrder.createdDate).toLocaleDateString()}</p>
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