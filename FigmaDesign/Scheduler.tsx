import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Input } from "./ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import { Calendar, Clock, Plus, Filter, Search, ArrowRight, Truck, Scissors, PackageOpen } from "lucide-react";
import { StatusChip } from "./StatusChip";

type WorkOrderPriority = "low" | "normal" | "high" | "urgent";
type WorkOrderStatus = "planned" | "ready" | "in-progress" | "paused" | "completed" | "error";
type MachineType = "ctl" | "slitter" | "picking";

interface WorkOrder {
  id: string;
  customerName: string;
  orderNumber: string;
  dueDate: string;
  priority: WorkOrderPriority;
  status: WorkOrderStatus;
  machineType: MachineType;
  estimatedHours: number;
  actualHours?: number;
  lineItems: {
    material: string;
    gauge: string;
    width: number;
    length: number;
    weight: number;
    color?: string;
    quantity: number;
  }[];
  assignedTo?: string;
  scheduledStart?: string;
  scheduledEnd?: string;
}

const machines = [
  { id: "ctl-01", name: "CTL Line 1", type: "ctl" as MachineType, status: "running" },
  { id: "ctl-02", name: "CTL Line 2", type: "ctl" as MachineType, status: "maintenance" },
  { id: "slitter-01", name: "Slitter 1", type: "slitter" as MachineType, status: "running" },
  { id: "slitter-02", name: "Slitter 2", type: "slitter" as MachineType, status: "idle" },
  { id: "pick-01", name: "Picking Station 1", type: "picking" as MachineType, status: "running" },
  { id: "pick-02", name: "Picking Station 2", type: "picking" as MachineType, status: "running" },
];

const sampleWorkOrders: WorkOrder[] = [
  {
    id: "WO-2024-0156",
    customerName: "Precision Manufacturing",
    orderNumber: "PM-8891",
    dueDate: "2024-01-18",
    priority: "high",
    status: "ready",
    machineType: "ctl",
    estimatedHours: 4.5,
    lineItems: [
      { material: "Cold Rolled Steel", gauge: "16ga", width: 48, length: 96, weight: 2850, color: "Galvanized", quantity: 25 }
    ],
    assignedTo: "Mike Rodriguez",
    scheduledStart: "2024-01-15T08:00",
    scheduledEnd: "2024-01-15T12:30"
  },
  {
    id: "WO-2024-0157",
    customerName: "ABC Construction",
    orderNumber: "ABC-4412",
    dueDate: "2024-01-19",
    priority: "normal",
    status: "planned",
    machineType: "slitter",
    estimatedHours: 6.0,
    lineItems: [
      { material: "Aluminum Sheet", gauge: "0.125", width: 60, length: 120, weight: 1240, quantity: 50 }
    ]
  },
  {
    id: "WO-2024-0158",
    customerName: "Industrial Fabricators",
    orderNumber: "IF-9923",
    dueDate: "2024-01-17",
    priority: "urgent",
    status: "in-progress",
    machineType: "ctl",
    estimatedHours: 8.0,
    actualHours: 3.2,
    lineItems: [
      { material: "Stainless Steel", gauge: "14ga", width: 36, length: 120, weight: 3680, color: "Mill Finish", quantity: 15 }
    ],
    assignedTo: "Sarah Chen",
    scheduledStart: "2024-01-15T06:00",
    scheduledEnd: "2024-01-15T14:00"
  }
];

const timeSlots = Array.from({ length: 24 }, (_, i) => {
  const hour = i.toString().padStart(2, '0');
  return `${hour}:00`;
});

const getMachineIcon = (type: MachineType) => {
  switch (type) {
    case "ctl": return <Truck className="h-4 w-4" />;
    case "slitter": return <Scissors className="h-4 w-4" />;
    case "picking": return <PackageOpen className="h-4 w-4" />;
  }
};

const getPriorityColor = (priority: WorkOrderPriority) => {
  switch (priority) {
    case "urgent": return "bg-red-100 text-red-800 border-red-200";
    case "high": return "bg-orange-100 text-orange-800 border-orange-200";
    case "normal": return "bg-blue-100 text-blue-800 border-blue-200";
    case "low": return "bg-gray-100 text-gray-800 border-gray-200";
  }
};

export function Scheduler() {
  const [selectedDate, setSelectedDate] = useState(new Date().toISOString().split('T')[0]);
  const [selectedMachine, setSelectedMachine] = useState("all");
  const [searchTerm, setSearchTerm] = useState("");
  const [draggedWorkOrder, setDraggedWorkOrder] = useState<WorkOrder | null>(null);

  const filteredWorkOrders = sampleWorkOrders.filter(wo => {
    const matchesMachine = selectedMachine === "all" || machines.some(m => m.type === wo.machineType && m.id === selectedMachine);
    const matchesSearch = wo.customerName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         wo.orderNumber.toLowerCase().includes(searchTerm.toLowerCase());
    return matchesMachine && matchesSearch;
  });

  const handleDragStart = (workOrder: WorkOrder) => {
    setDraggedWorkOrder(workOrder);
  };

  const handleDrop = (machineId: string, timeSlot: string) => {
    if (draggedWorkOrder) {
      console.log(`Scheduling ${draggedWorkOrder.id} on ${machineId} at ${timeSlot}`);
      // Here you would update the work order's schedule
      setDraggedWorkOrder(null);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1>Production Scheduler</h1>
        <p className="text-muted-foreground">Plan and schedule work orders across machines</p>
      </div>

      {/* Controls */}
      <div className="flex flex-wrap gap-4 items-center justify-between">
        <div className="flex gap-2 items-center">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search work orders..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10 w-64"
            />
          </div>
          <Select value={selectedMachine} onValueChange={setSelectedMachine}>
            <SelectTrigger className="w-48">
              <SelectValue placeholder="Filter by machine" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Machines</SelectItem>
              {machines.map(machine => (
                <SelectItem key={machine.id} value={machine.id}>
                  {machine.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="flex gap-2 items-center">
          <Input
            type="date"
            value={selectedDate}
            onChange={(e) => setSelectedDate(e.target.value)}
            className="w-40"
          />
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            New Work Order
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
        {/* Unscheduled Work Orders */}
        <div className="lg:col-span-1 space-y-4">
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-base flex items-center gap-2">
                <Filter className="h-4 w-4" />
                Unscheduled Orders
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              {filteredWorkOrders
                .filter(wo => !wo.scheduledStart)
                .map(workOrder => (
                <div
                  key={workOrder.id}
                  draggable
                  onDragStart={() => handleDragStart(workOrder)}
                  className="p-3 border rounded-lg cursor-move hover:bg-gray-50 transition-colors"
                >
                  <div className="flex items-start justify-between mb-2">
                    <div className="flex items-center gap-2">
                      {getMachineIcon(workOrder.machineType)}
                      <span className="font-medium text-sm">{workOrder.id}</span>
                    </div>
                    <Badge variant="outline" className={getPriorityColor(workOrder.priority)}>
                      {workOrder.priority}
                    </Badge>
                  </div>
                  
                  <div className="space-y-1">
                    <p className="text-sm">{workOrder.customerName}</p>
                    <p className="text-xs text-muted-foreground">{workOrder.orderNumber}</p>
                    <div className="flex items-center gap-2 text-xs text-muted-foreground">
                      <Clock className="h-3 w-3" />
                      {workOrder.estimatedHours}h
                      <Calendar className="h-3 w-3" />
                      Due {workOrder.dueDate}
                    </div>
                  </div>
                  
                  <StatusChip status={workOrder.status} />
                </div>
              ))}
            </CardContent>
          </Card>
        </div>

        {/* Schedule Grid */}
        <div className="lg:col-span-3">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center justify-between">
                <span>Production Schedule - {selectedDate}</span>
                <div className="flex items-center gap-4 text-sm">
                  <div className="flex items-center gap-2">
                    <div className="w-3 h-3 bg-teal-500 rounded-full"></div>
                    <span>Scheduled</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="w-3 h-3 bg-green-500 rounded-full"></div>
                    <span>In Progress</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="w-3 h-3 bg-yellow-500 rounded-full"></div>
                    <span>Paused</span>
                  </div>
                </div>
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-[200px_1fr] gap-0 border rounded-lg overflow-hidden">
                {/* Header */}
                <div className="bg-gray-50 p-3 border-r border-b">
                  <span className="font-medium">Machine</span>
                </div>
                <div className="bg-gray-50 grid grid-cols-24 border-b">
                  {timeSlots.slice(0, 24).map((slot, index) => (
                    <div key={slot} className="p-1 text-xs text-center border-r last:border-r-0">
                      {index % 4 === 0 ? slot : ""}
                    </div>
                  ))}
                </div>

                {/* Machine Rows */}
                {machines.map(machine => (
                  <div key={machine.id} className="contents">
                    <div className="p-3 border-r border-b bg-white">
                      <div className="flex items-center gap-2">
                        {getMachineIcon(machine.type)}
                        <div>
                          <p className="font-medium text-sm">{machine.name}</p>
                          <Badge 
                            variant={machine.status === "running" ? "default" : machine.status === "maintenance" ? "destructive" : "secondary"}
                            className="text-xs"
                          >
                            {machine.status}
                          </Badge>
                        </div>
                      </div>
                    </div>
                    
                    <div 
                      className="grid grid-cols-24 border-b min-h-[80px] relative"
                      onDragOver={(e) => e.preventDefault()}
                      onDrop={(e) => {
                        e.preventDefault();
                        const rect = e.currentTarget.getBoundingClientRect();
                        const x = e.clientX - rect.left;
                        const slotWidth = rect.width / 24;
                        const slotIndex = Math.floor(x / slotWidth);
                        const timeSlot = timeSlots[slotIndex];
                        handleDrop(machine.id, timeSlot);
                      }}
                    >
                      {timeSlots.map((slot, index) => (
                        <div 
                          key={`${machine.id}-${slot}`}
                          className="border-r last:border-r-0 hover:bg-blue-50 transition-colors"
                        />
                      ))}
                      
                      {/* Scheduled Work Orders */}
                      {filteredWorkOrders
                        .filter(wo => wo.scheduledStart && wo.machineType === machine.type)
                        .map(workOrder => {
                          const startHour = workOrder.scheduledStart ? parseInt(workOrder.scheduledStart.split('T')[1].split(':')[0]) : 0;
                          const duration = workOrder.estimatedHours;
                          const leftPercent = (startHour / 24) * 100;
                          const widthPercent = (duration / 24) * 100;
                          
                          return (
                            <div
                              key={workOrder.id}
                              className={`absolute top-2 bottom-2 rounded px-2 py-1 text-xs cursor-pointer
                                ${workOrder.status === 'in-progress' ? 'bg-green-500 text-white' :
                                  workOrder.status === 'paused' ? 'bg-yellow-500 text-white' :
                                  'bg-teal-500 text-white'}`}
                              style={{
                                left: `${leftPercent}%`,
                                width: `${widthPercent}%`,
                                minWidth: '80px'
                              }}
                            >
                              <div className="font-medium">{workOrder.id}</div>
                              <div className="opacity-90">{workOrder.customerName}</div>
                              {workOrder.assignedTo && (
                                <div className="opacity-75">→ {workOrder.assignedTo}</div>
                              )}
                            </div>
                          );
                        })}
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Quick Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center">
              <div className="p-2 bg-teal-100 rounded-full">
                <Calendar className="h-4 w-4 text-teal-600" />
              </div>
              <div className="ml-4">
                <p className="text-sm text-muted-foreground">Scheduled Today</p>
                <p className="text-2xl font-medium">
                  {filteredWorkOrders.filter(wo => wo.scheduledStart?.startsWith(selectedDate)).length}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center">
              <div className="p-2 bg-orange-100 rounded-full">
                <Clock className="h-4 w-4 text-orange-600" />
              </div>
              <div className="ml-4">
                <p className="text-sm text-muted-foreground">Unscheduled</p>
                <p className="text-2xl font-medium">
                  {filteredWorkOrders.filter(wo => !wo.scheduledStart).length}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center">
              <div className="p-2 bg-green-100 rounded-full">
                <ArrowRight className="h-4 w-4 text-green-600" />
              </div>
              <div className="ml-4">
                <p className="text-sm text-muted-foreground">In Progress</p>
                <p className="text-2xl font-medium">
                  {filteredWorkOrders.filter(wo => wo.status === 'in-progress').length}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center">
              <div className="p-2 bg-red-100 rounded-full">
                <Filter className="h-4 w-4 text-red-600" />
              </div>
              <div className="ml-4">
                <p className="text-sm text-muted-foreground">Overdue</p>
                <p className="text-2xl font-medium">
                  {filteredWorkOrders.filter(wo => new Date(wo.dueDate) < new Date() && wo.status !== 'completed').length}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}