import { useState, useEffect } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { Textarea } from "./ui/textarea";
import { Progress } from "./ui/progress";
import { Separator } from "./ui/separator";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./ui/select";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "./ui/dialog";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "./ui/alert-dialog";
import {
  Save,
  ArrowLeft,
  CheckCircle,
  Settings,
  Clock,
  FileText,
  Tag,
  Play,
  Pause,
  Weight,
  AlertTriangle,
  Activity,
  Package,
  Layers,
  Target,
  Split,
  Info,
  X
} from "lucide-react";
import { toast } from "sonner@2.0.3";

interface WorkOrderFlatProps {
  mode: "create" | "process";
  workOrderId?: string;
  onNavigate?: (page: string) => void;
  onSave?: (workOrder: any) => void;
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

// Interfaces
interface Machine {
  id: number;
  name: string;
  category: MachineCategory;
  isActive: boolean;
  estimatedLbsPerHour: number;
  currentStatus: "idle" | "running" | "maintenance" | "error";
  maxSkidCapacity: number; // lbs
}

interface MasterCoil {
  id: string;
  tagNumber: string;
  itemId: string; // For Slitter matching
  material: string;
  gauge: string;
  width: number;
  totalWeight: number;
  remainingWeight: number;
  location: string;
  heatNumber: string;
  qualityGrade: string;
}

interface ChildItem {
  id: string;
  masterCoilId: string;
  itemCode: string;
  description: string;
  width: number;
  length: number;
  gauge: string;
  material: string;
  targetWeight: number;
  canProduceFrom: string[]; // coil IDs that can produce this item
}

interface Customer {
  id: string;
  name: string;
  maxSkidCapacity: number; // Customer-specific max skid capacity in lbs
  preferredDeliveryTime: string;
  specialInstructions?: string;
}

interface SalesOrder {
  id: string;
  orderNumber: string;
  customerName: string;
  customerId: string;
  customerPO?: string;
  dueDate: string;
  priority: "low" | "normal" | "high" | "urgent";
  lineItems: OrderLineItem[];
}

interface OrderLineItem {
  id: string;
  salesOrderId: string;
  itemCode: string;
  itemId?: string; // For Slitter matching - matches coil itemId
  description: string;
  orderQuantity: number;
  orderWeight: number;
  remainingQuantity: number;
  remainingWeight: number;
  width: number;
  length: number;
  gauge: string;
  material: string;
  location?: string;
  childItemId?: string; // Links to ChildItem (for CTL)
}

interface WorkOrderLineItem {
  id: string;
  workOrderId: string;
  salesOrderId: string;
  orderLineItemId: string;
  customerName: string;
  customerId: string;
  customerMaxSkidCapacity: number;
  orderNumber: string;
  itemCode: string;
  description: string;
  plannedQuantity: number;
  plannedWeight: number;
  processedQuantity: number;
  processedWeight: number;
  status: "pending" | "in-progress" | "completed" | "split";
  workOrderSequence: number; // For multi-work order scenarios
  splitReason?: "skid-capacity" | "coil-capacity" | "customer-request";
  manuallyAdjusted: boolean; // Track if user manually modified
}

interface WorkOrderForm {
  id?: string;
  workOrderNumber?: string;
  tagNumber: string;
  machineId?: number;
  machine?: Machine;
  masterCoilId?: string;
  masterCoil?: MasterCoil;
  
  // Dates
  dueDate: string;
  scheduledStartDate: string;
  scheduledEndDate: string;
  actualStartDate?: string;
  actualEndDate?: string;
  
  // Items and matching
  matchedOrders: SalesOrder[];
  workOrderLineItems: WorkOrderLineItem[];
  
  // Instructions & Status
  instructions?: string;
  priority: "low" | "normal" | "high" | "urgent";
  status: WorkOrderStatus;
  
  // Multi-work order tracking
  isMultiWorkOrder: boolean;
  totalWorkOrders: number;
  workOrderSequence: number;
  
  // Processing Info
  operator?: string;
  actualLbs?: number;
  estimatedDuration?: number;
  
  // Audit
  createdBy: string;
  createdDate: string;
  lastUpdatedDate: string;
  
  // Events
  events: Array<{
    type: "created" | "started" | "paused" | "resumed" | "completed" | "split" | "error";
    timestamp: string;
    operator: string;
    notes?: string;
  }>;
}

// Mock data
const customers: Customer[] = [
  {
    id: "CUST-001",
    name: "Industrial Metals Co",
    maxSkidCapacity: 3500, // Lower capacity customer
    preferredDeliveryTime: "morning",
    specialInstructions: "Forklift access limited to 3500 lbs"
  },
  {
    id: "CUST-002", 
    name: "Precision Parts LLC",
    maxSkidCapacity: 5000, // Standard capacity
    preferredDeliveryTime: "afternoon"
  },
  {
    id: "CUST-003",
    name: "Metro Construction",
    maxSkidCapacity: 6000, // Higher capacity customer
    preferredDeliveryTime: "any",
    specialInstructions: "Large receiving dock, can handle heavy skids"
  },
  {
    id: "CUST-004",
    name: "Small Shop LLC",
    maxSkidCapacity: 2000, // Very limited capacity
    preferredDeliveryTime: "morning",
    specialInstructions: "Manual unloading only, small skids required"
  }
];

const machines: Machine[] = [
  { id: 1, name: "CTL Line 1", category: MachineCategory.CTL, isActive: true, estimatedLbsPerHour: 1200, currentStatus: "idle", maxSkidCapacity: 4000 },
  { id: 2, name: "CTL Line 2", category: MachineCategory.CTL, isActive: true, estimatedLbsPerHour: 1100, currentStatus: "running", maxSkidCapacity: 4000 },
  { id: 3, name: "Slitter 1", category: MachineCategory.Slitter, isActive: true, estimatedLbsPerHour: 800, currentStatus: "idle", maxSkidCapacity: 3500 },
  { id: 4, name: "Slitter 2", category: MachineCategory.Slitter, isActive: true, estimatedLbsPerHour: 900, currentStatus: "maintenance", maxSkidCapacity: 3500 },
  { id: 5, name: "Picking Line 1", category: MachineCategory.Picking, isActive: true, estimatedLbsPerHour: 1500, currentStatus: "running", maxSkidCapacity: 5000 },
  { id: 6, name: "Packing Station 1", category: MachineCategory.Packing, isActive: true, estimatedLbsPerHour: 1000, currentStatus: "idle", maxSkidCapacity: 2000 }
];

const masterCoils: MasterCoil[] = [
  {
    id: "MC-001",
    tagNumber: "HRC-24-48-001",
    itemId: "HR-16-48-COIL", // For Slitter matching
    material: "Hot Rolled Steel", 
    gauge: "16 GA",
    width: 48,
    totalWeight: 25000,
    remainingWeight: 18500,
    location: "A-15-02",
    heatNumber: "H123456",
    qualityGrade: "Commercial"
  },
  {
    id: "MC-002", 
    tagNumber: "CRC-20-36-002",
    itemId: "CR-20-36-COIL", // For Slitter matching
    material: "Cold Rolled Steel",
    gauge: "20 GA", 
    width: 36,
    totalWeight: 20000,
    remainingWeight: 15200,
    location: "B-08-05",
    heatNumber: "H234567",
    qualityGrade: "Drawing Quality"
  },
  {
    id: "MC-003",
    tagNumber: "GAL-18-60-003",
    itemId: "GAL-18-60-COIL", // For Slitter matching
    material: "Galvanized Steel",
    gauge: "18 GA",
    width: 60,
    totalWeight: 30000,
    remainingWeight: 22400,
    location: "C-12-08",
    heatNumber: "H345678",
    qualityGrade: "Commercial"
  }
];

const childItems: ChildItem[] = [
  {
    id: "CI-001",
    masterCoilId: "MC-001",
    itemCode: "HR-16-48-96",
    description: "Hot Rolled Sheet - 16 GA x 48\" x 96\"",
    width: 48,
    length: 96,
    gauge: "16 GA",
    material: "Hot Rolled Steel",
    targetWeight: 320,
    canProduceFrom: ["MC-001"]
  },
  {
    id: "CI-002", 
    masterCoilId: "MC-001",
    itemCode: "HR-16-48-120",
    description: "Hot Rolled Sheet - 16 GA x 48\" x 120\"",
    width: 48,
    length: 120,
    gauge: "16 GA", 
    material: "Hot Rolled Steel",
    targetWeight: 400,
    canProduceFrom: ["MC-001"]
  },
  {
    id: "CI-003",
    masterCoilId: "MC-002",
    itemCode: "CR-20-36-144",
    description: "Cold Rolled Sheet - 20 GA x 36\" x 144\"", 
    width: 36,
    length: 144,
    gauge: "20 GA",
    material: "Cold Rolled Steel",
    targetWeight: 280,
    canProduceFrom: ["MC-002"]
  }
];

const salesOrders: SalesOrder[] = [
  {
    id: "SO-45621",
    orderNumber: "SO-45621",
    customerName: "Industrial Metals Co",
    customerId: "CUST-001",
    customerPO: "PO-12345", 
    dueDate: "2024-12-15",
    priority: "normal",
    lineItems: [
      {
        id: "LI-001",
        salesOrderId: "SO-45621",
        itemCode: "HR-16-48-96",
        itemId: "HR-16-48-COIL", // For Slitter matching
        description: "Hot Rolled Sheet - 16 GA x 48\" x 96\"",
        orderQuantity: 15,
        orderWeight: 4800,
        remainingQuantity: 15,
        remainingWeight: 4800,
        width: 48,
        length: 96,
        gauge: "16 GA",
        material: "Hot Rolled Steel",
        location: "A-15-02",
        childItemId: "CI-001"
      },
      {
        id: "LI-002",
        salesOrderId: "SO-45621", 
        itemCode: "HR-16-48-120",
        itemId: "HR-16-48-COIL", // For Slitter matching
        description: "Hot Rolled Sheet - 16 GA x 48\" x 120\"",
        orderQuantity: 8,
        orderWeight: 3200,
        remainingQuantity: 8,
        remainingWeight: 3200,
        width: 48,
        length: 120,
        gauge: "16 GA",
        material: "Hot Rolled Steel",
        location: "A-15-02",
        childItemId: "CI-002"
      }
    ]
  },
  {
    id: "SO-45622",
    orderNumber: "SO-45622", 
    customerName: "Precision Parts LLC",
    customerId: "CUST-002",
    customerPO: "PO-67890",
    dueDate: "2024-12-13",
    priority: "high",
    lineItems: [
      {
        id: "LI-003",
        salesOrderId: "SO-45622",
        itemCode: "HR-16-48-96",
        itemId: "HR-16-48-COIL", // For Slitter matching
        description: "Hot Rolled Sheet - 16 GA x 48\" x 96\"",
        orderQuantity: 25,
        orderWeight: 8000,
        remainingQuantity: 25,
        remainingWeight: 8000,
        width: 48,
        length: 96,
        gauge: "16 GA",
        material: "Hot Rolled Steel",
        location: "A-15-02",
        childItemId: "CI-001"
      }
    ]
  },
  {
    id: "SO-45623",
    orderNumber: "SO-45623",
    customerName: "Metro Construction", 
    customerId: "CUST-003",
    customerPO: "PO-98765",
    dueDate: "2024-12-14",
    priority: "normal",
    lineItems: [
      {
        id: "LI-004",
        salesOrderId: "SO-45623",
        itemCode: "CR-20-36-144",
        itemId: "CR-20-36-COIL", // For Slitter matching
        description: "Cold Rolled Sheet - 20 GA x 36\" x 144\"",
        orderQuantity: 20,
        orderWeight: 5600,
        remainingQuantity: 20,
        remainingWeight: 5600,
        width: 36,
        length: 144,
        gauge: "20 GA",
        material: "Cold Rolled Steel",
        location: "B-08-05",
        childItemId: "CI-003"
      }
    ]
  },
  {
    id: "SO-45624",
    orderNumber: "SO-45624",
    customerName: "Small Shop LLC", 
    customerId: "CUST-004",
    customerPO: "PO-11111",
    dueDate: "2024-12-16",
    priority: "normal",
    lineItems: [
      {
        id: "LI-005",
        salesOrderId: "SO-45624",
        itemCode: "HR-16-48-72",
        itemId: "HR-16-48-COIL", // For Slitter matching
        description: "Hot Rolled Sheet - 16 GA x 48\" x 72\"",
        orderQuantity: 10,
        orderWeight: 2400,
        remainingQuantity: 10,
        remainingWeight: 2400,
        width: 48,
        length: 72,
        gauge: "16 GA",
        material: "Hot Rolled Steel",
        location: "A-15-02"
      }
    ]
  }
];

// Initialize form
const createInitialForm = (mode: "create" | "process", workOrderId?: string): WorkOrderForm => {
  const now = new Date().toISOString();
  
  if (mode === "process" && workOrderId) {
    return {
      id: "WO-2024001",
      workOrderNumber: "WO-2024001",
      tagNumber: "HRC-24-48-001",
      machineId: 1,
      machine: machines[0],
      masterCoilId: "MC-001",
      masterCoil: masterCoils[0],
      dueDate: "2024-12-15",
      scheduledStartDate: "2024-12-13T09:00:00",
      scheduledEndDate: "2024-12-13T11:30:00",
      matchedOrders: [salesOrders[0], salesOrders[1]],
      workOrderLineItems: [
        {
          id: "WOI-001",
          workOrderId: "WO-2024001",
          salesOrderId: "SO-45621",
          orderLineItemId: "LI-001",
          customerName: "Industrial Metals Co",
          customerId: "CUST-001",
          customerMaxSkidCapacity: 3500,
          orderNumber: "SO-45621",
          itemCode: "HR-16-48-96",
          description: "Hot Rolled Sheet - 16 GA x 48\" x 96\"",
          plannedQuantity: 11, // Reduced due to customer skid capacity
          plannedWeight: 3520, // Adjusted for customer capacity
          processedQuantity: 0,
          processedWeight: 0,
          status: "pending",
          workOrderSequence: 1,
          splitReason: "skid-capacity",
          manuallyAdjusted: false
        }
      ],
      instructions: "Cut to length according to specifications. Check dimensions carefully.",
      priority: "normal",
      status: WorkOrderStatus.Pending,
      isMultiWorkOrder: false,
      totalWorkOrders: 1,
      workOrderSequence: 1,
      operator: "Mike Johnson",
      createdBy: "Sarah Planning",
      createdDate: "2024-12-12T16:30:00Z",
      lastUpdatedDate: "2024-12-12T16:30:00Z",
      events: [
        { type: "created", timestamp: "2024-12-12T16:30:00Z", operator: "Sarah Planning" }
      ]
    };
  }

  return {
    tagNumber: "",
    dueDate: "",
    scheduledStartDate: "",
    scheduledEndDate: "",
    matchedOrders: [],
    workOrderLineItems: [],
    instructions: "",
    priority: "normal",
    status: WorkOrderStatus.Draft,
    isMultiWorkOrder: false,
    totalWorkOrders: 1,
    workOrderSequence: 1,
    createdBy: "Current User",
    createdDate: now,
    lastUpdatedDate: now,
    events: []
  };
};

export function WorkOrderFlat({ 
  mode, 
  workOrderId, 
  onNavigate, 
  onSave 
}: WorkOrderFlatProps) {
  const [form, setForm] = useState<WorkOrderForm>(() => createInitialForm(mode, workOrderId));
  const [isProcessing, setIsProcessing] = useState(false);
  const [elapsedTime, setElapsedTime] = useState(0);
  const [pauseReason, setPauseReason] = useState("");
  const [qualityNotes, setQualityNotes] = useState("");
  const [actualLbsInput, setActualLbsInput] = useState("");
  const [showSplitDetails, setShowSplitDetails] = useState(false);

  // Timer effect for processing mode
  useEffect(() => {
    let interval: NodeJS.Timeout;
    if (mode === "process" && isProcessing && form.status === WorkOrderStatus.InProgress) {
      interval = setInterval(() => {
        setElapsedTime(prev => prev + 1);
      }, 1000);
    }
    return () => clearInterval(interval);
  }, [mode, isProcessing, form.status]);

  const updateForm = (field: keyof WorkOrderForm, value: any) => {
    setForm(prev => ({ ...prev, [field]: value, lastUpdatedDate: new Date().toISOString() }));
  };

  // Find matching orders when machine and tag number are selected
  useEffect(() => {
    if (form.machineId && form.tagNumber && form.machine) {
      const coil = masterCoils.find(c => c.tagNumber === form.tagNumber);
      if (coil) {
        updateForm("masterCoilId", coil.id);
        updateForm("masterCoil", coil);
        
        let matchedOrders: SalesOrder[] = [];
        
        if (form.machine.category === MachineCategory.CTL) {
          // CTL: Find child items that can be produced from this coil
          const availableChildItems = childItems.filter(ci => ci.canProduceFrom.includes(coil.id));
          
          // Find orders that match these child items
          matchedOrders = salesOrders.filter(so => 
            so.lineItems.some(li => 
              availableChildItems.some(ci => ci.itemCode === li.itemCode)
            )
          );
        } else if (form.machine.category === MachineCategory.Slitter) {
          // Slitter: Find orders where line item itemId matches coil itemId
          matchedOrders = salesOrders.filter(so => 
            so.lineItems.some(li => li.itemId === coil.itemId)
          );
        }
        
        updateForm("matchedOrders", matchedOrders);
        
        // Auto-create work order line items and handle splitting
        const lineItems = createWorkOrderLineItems(matchedOrders, coil);
        updateForm("workOrderLineItems", lineItems);
        
        toast.success(`Found ${matchedOrders.length} matching orders for ${form.machine.category} on coil ${form.tagNumber}`);
      } else {
        toast.error(`No coil found with tag number ${form.tagNumber}`);
        updateForm("matchedOrders", []);
        updateForm("workOrderLineItems", []);
      }
    }
  }, [form.machineId, form.tagNumber]);

  const createWorkOrderLineItems = (orders: SalesOrder[], coil: MasterCoil): WorkOrderLineItem[] => {
    const machine = form.machine;
    if (!machine) return [];

    const lineItems: WorkOrderLineItem[] = [];
    let workOrderSequence = 1;
    let remainingCoilWeight = coil.remainingWeight;

    orders.forEach(order => {
      // Get customer for this order
      const customer = customers.find(c => c.id === order.customerId);
      const customerMaxSkidCapacity = customer?.maxSkidCapacity || machine.maxSkidCapacity;

      order.lineItems.forEach(lineItem => {
        // Check if this line item matches the coil
        let canProcess = false;
        
        if (machine.category === MachineCategory.CTL) {
          const childItem = childItems.find(ci => ci.itemCode === lineItem.itemCode);
          canProcess = childItem && childItem.canProduceFrom.includes(coil.id);
        } else if (machine.category === MachineCategory.Slitter) {
          canProcess = lineItem.itemId === coil.itemId;
        }

        if (!canProcess) return;

        let remainingOrderWeight = lineItem.remainingWeight;
        let remainingOrderQuantity = lineItem.remainingQuantity;

        while (remainingOrderWeight > 0 && remainingCoilWeight > 0) {
          // Determine how much we can process in this work order
          // Use customer-specific max skid capacity
          const maxBySkidCapacity = Math.min(machine.maxSkidCapacity, customerMaxSkidCapacity);
          const maxByCoilRemaining = remainingCoilWeight;
          const maxByOrderRemaining = remainingOrderWeight;
          
          const plannedWeight = Math.min(maxBySkidCapacity, maxByCoilRemaining, maxByOrderRemaining);
          const plannedQuantity = Math.floor((plannedWeight / lineItem.orderWeight) * lineItem.orderQuantity);
          
          // Determine split reason
          let splitReason: "skid-capacity" | "coil-capacity" | undefined;
          if (plannedWeight < remainingOrderWeight) {
            if (plannedWeight === maxBySkidCapacity) {
              splitReason = "skid-capacity";
            } else if (plannedWeight === maxByCoilRemaining) {
              splitReason = "coil-capacity";
            }
          }

          const workOrderLineItem: WorkOrderLineItem = {
            id: `WOI-${Date.now()}-${workOrderSequence}`,
            workOrderId: form.id || "",
            salesOrderId: order.id,
            orderLineItemId: lineItem.id,
            customerName: order.customerName,
            customerId: order.customerId,
            customerMaxSkidCapacity,
            orderNumber: order.orderNumber,
            itemCode: lineItem.itemCode,
            description: lineItem.description,
            plannedQuantity,
            plannedWeight,
            processedQuantity: 0,
            processedWeight: 0,
            status: "pending",
            workOrderSequence,
            splitReason,
            manuallyAdjusted: false
          };

          lineItems.push(workOrderLineItem);

          // Update remaining amounts
          remainingOrderWeight -= plannedWeight;
          remainingOrderQuantity -= plannedQuantity;
          remainingCoilWeight -= plannedWeight;
          workOrderSequence++;

          // Check if we need multiple work orders
          if (remainingOrderWeight > 0) {
            updateForm("isMultiWorkOrder", true);
            updateForm("totalWorkOrders", workOrderSequence);
          }
        }
      });
    });

    return lineItems;
  };

  const selectMachine = (machine: Machine) => {
    updateForm("machineId", machine.id);
    updateForm("machine", machine);
  };

  const updateLineItemQuantity = (lineItemId: string, newQuantity: number) => {
    const updatedLineItems = form.workOrderLineItems.map(item => {
      if (item.id === lineItemId) {
        // Calculate new weight based on quantity
        const originalLineItem = form.matchedOrders
          .flatMap(order => order.lineItems)
          .find(li => li.id === item.orderLineItemId);
        
        if (originalLineItem) {
          const unitWeight = originalLineItem.orderWeight / originalLineItem.orderQuantity;
          const newWeight = newQuantity * unitWeight;
          
          return {
            ...item,
            plannedQuantity: newQuantity,
            plannedWeight: newWeight,
            manuallyAdjusted: true
          };
        }
      }
      return item;
    });
    
    updateForm("workOrderLineItems", updatedLineItems);
  };

  const updateLineItemWeight = (lineItemId: string, newWeight: number) => {
    const updatedLineItems = form.workOrderLineItems.map(item => {
      if (item.id === lineItemId) {
        // Calculate new quantity based on weight
        const originalLineItem = form.matchedOrders
          .flatMap(order => order.lineItems)
          .find(li => li.id === item.orderLineItemId);
        
        if (originalLineItem) {
          const unitWeight = originalLineItem.orderWeight / originalLineItem.orderQuantity;
          const newQuantity = Math.floor(newWeight / unitWeight);
          
          return {
            ...item,
            plannedQuantity: newQuantity,
            plannedWeight: newWeight,
            manuallyAdjusted: true
          };
        }
      }
      return item;
    });
    
    updateForm("workOrderLineItems", updatedLineItems);
  };

  const removeLineItem = (lineItemId: string) => {
    const updatedLineItems = form.workOrderLineItems.filter(item => item.id !== lineItemId);
    updateForm("workOrderLineItems", updatedLineItems);
    toast.success("Line item removed from work order");
  };

  const formatTime = (seconds: number) => {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;
    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  const calculateProgress = () => {
    const totalWeight = form.workOrderLineItems.reduce((sum, item) => sum + item.plannedWeight, 0);
    const processedWeight = form.workOrderLineItems.reduce((sum, item) => sum + item.processedWeight, 0);
    return totalWeight > 0 ? (processedWeight / totalWeight) * 100 : 0;
  };

  const startWorkOrder = () => {
    const now = new Date().toISOString();
    updateForm("status", WorkOrderStatus.InProgress);
    updateForm("actualStartDate", now);
    updateForm("operator", "Mike Johnson");
    updateForm("events", [...form.events, {
      type: "started",
      timestamp: now,
      operator: "Mike Johnson"
    }]);
    setIsProcessing(true);
    toast.success("Work order started");
  };

  const pauseWorkOrder = (reason: string) => {
    const now = new Date().toISOString();
    updateForm("status", WorkOrderStatus.Awaiting);
    updateForm("events", [...form.events, {
      type: "paused",
      timestamp: now,
      operator: form.operator || "Current User",
      notes: reason
    }]);
    setIsProcessing(false);
    setPauseReason("");
    toast.success("Work order paused");
  };

  const resumeWorkOrder = () => {
    const now = new Date().toISOString();
    updateForm("status", WorkOrderStatus.InProgress);
    updateForm("events", [...form.events, {
      type: "resumed",
      timestamp: now,
      operator: form.operator || "Current User"
    }]);
    setIsProcessing(true);
    toast.success("Work order resumed");
  };

  const completeWorkOrder = () => {
    const now = new Date().toISOString();
    updateForm("status", WorkOrderStatus.Completed);
    updateForm("actualEndDate", now);
    updateForm("actualLbs", parseInt(actualLbsInput) || getTotalWeight());
    updateForm("events", [...form.events, {
      type: "completed",
      timestamp: now,
      operator: form.operator || "Current User",
      notes: qualityNotes
    }]);
    setIsProcessing(false);
    toast.success("Work order completed");
  };

  const saveWorkOrder = () => {
    if (mode === "create") {
      const woNumber = `WO-${Date.now()}`;
      const updatedForm = {
        ...form,
        id: woNumber,
        workOrderNumber: woNumber,
        events: [{
          type: "created" as const,
          timestamp: new Date().toISOString(),
          operator: form.createdBy
        }]
      };
      setForm(updatedForm);
      onSave?.(updatedForm);
      toast.success(`Work order ${woNumber} created successfully`);
    } else {
      onSave?.(form);
      toast.success("Work order updated successfully");
    }
  };

  const getTotalWeight = () => {
    return form.workOrderLineItems.reduce((sum, item) => sum + item.plannedWeight, 0);
  };

  const currentProgress = calculateProgress();
  const priorityColors = {
    low: "bg-gray-100 text-gray-700",
    normal: "bg-blue-100 text-blue-700",
    high: "bg-orange-100 text-orange-700",
    urgent: "bg-red-100 text-red-700"
  };

  const statusColors = {
    [WorkOrderStatus.Draft]: "bg-gray-100 text-gray-700",
    [WorkOrderStatus.Pending]: "bg-blue-100 text-blue-700",
    [WorkOrderStatus.InProgress]: "bg-green-100 text-green-700",
    [WorkOrderStatus.Awaiting]: "bg-yellow-100 text-yellow-700",
    [WorkOrderStatus.Completed]: "bg-teal-100 text-teal-700",
    [WorkOrderStatus.Canceled]: "bg-red-100 text-red-700"
  };

  const machineStatusColors = {
    idle: "bg-gray-100 text-gray-700",
    running: "bg-green-100 text-green-700",
    maintenance: "bg-yellow-100 text-yellow-700",
    error: "bg-red-100 text-red-700"
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button 
            variant="ghost" 
            size="sm"
            onClick={() => onNavigate?.("work-orders")}
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Work Orders
          </Button>
          <div>
            <div className="flex items-center gap-2">
              <h1>{mode === "create" ? "Create Work Order" : form.workOrderNumber}</h1>
              {form.status && (
                <div className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${statusColors[form.status]}`}>
                  {form.status}
                </div>
              )}
              {form.priority && (
                <Badge className={priorityColors[form.priority]}>{form.priority}</Badge>
              )}
              {form.isMultiWorkOrder && (
                <Badge variant="outline" className="bg-orange-100 text-orange-700">
                  Multi WO {form.workOrderSequence}/{form.totalWorkOrders}
                </Badge>
              )}
            </div>
            <p className="text-muted-foreground">
              {mode === "create" 
                ? "Simplified work order creation with automatic order matching" 
                : `${form.machine?.name} • ${form.masterCoil?.tagNumber} • ${getTotalWeight().toLocaleString()} lbs`
              }
            </p>
          </div>
        </div>

        <div className="flex items-center gap-2">
          {/* Processing Controls */}
          {mode === "process" && (
            <>
              {form.status === WorkOrderStatus.Pending && (
                <Button onClick={startWorkOrder} className="bg-teal-600 hover:bg-teal-700">
                  <Play className="h-4 w-4 mr-2" />
                  Start Work Order
                </Button>
              )}
              
              {form.status === WorkOrderStatus.InProgress && (
                <>
                  <Dialog>
                    <DialogTrigger asChild>
                      <Button variant="outline">
                        <Pause className="h-4 w-4 mr-2" />
                        Pause
                      </Button>
                    </DialogTrigger>
                    <DialogContent>
                      <DialogHeader>
                        <DialogTitle>Pause Work Order</DialogTitle>
                        <DialogDescription>Please provide a reason for pausing this work order</DialogDescription>
                      </DialogHeader>
                      <div className="space-y-4">
                        <div className="space-y-2">
                          <Label>Reason for Pause</Label>
                          <Select value={pauseReason} onValueChange={setPauseReason}>
                            <SelectTrigger>
                              <SelectValue placeholder="Select reason" />
                            </SelectTrigger>
                            <SelectContent>
                              <SelectItem value="break">Break</SelectItem>
                              <SelectItem value="maintenance">Machine Maintenance</SelectItem>
                              <SelectItem value="material">Material Issue</SelectItem>
                              <SelectItem value="quality">Quality Check</SelectItem>
                              <SelectItem value="coil-change">Coil Change Required</SelectItem>
                              <SelectItem value="other">Other</SelectItem>
                            </SelectContent>
                          </Select>
                        </div>
                        <Button 
                          onClick={() => pauseWorkOrder(pauseReason)} 
                          disabled={!pauseReason}
                          className="w-full"
                        >
                          Pause Work Order
                        </Button>
                      </div>
                    </DialogContent>
                  </Dialog>

                  <AlertDialog>
                    <AlertDialogTrigger asChild>
                      <Button className="bg-green-600 hover:bg-green-700">
                        <CheckCircle className="h-4 w-4 mr-2" />
                        Complete
                      </Button>
                    </AlertDialogTrigger>
                    <AlertDialogContent>
                      <AlertDialogHeader>
                        <AlertDialogTitle>Complete Work Order</AlertDialogTitle>
                        <AlertDialogDescription>
                          Are you sure you want to mark this work order as complete?
                        </AlertDialogDescription>
                      </AlertDialogHeader>
                      <div className="space-y-4 my-4">
                        <div className="space-y-2">
                          <Label>Actual Pounds Processed</Label>
                          <Input
                            type="number"
                            placeholder={getTotalWeight().toString()}
                            value={actualLbsInput}
                            onChange={(e) => setActualLbsInput(e.target.value)}
                          />
                        </div>
                        <div className="space-y-2">
                          <Label>Quality Notes (Optional)</Label>
                          <Textarea
                            placeholder="Any quality observations or notes..."
                            value={qualityNotes}
                            onChange={(e) => setQualityNotes(e.target.value)}
                            rows={3}
                          />
                        </div>
                      </div>
                      <AlertDialogFooter>
                        <AlertDialogCancel>Cancel</AlertDialogCancel>
                        <AlertDialogAction onClick={completeWorkOrder}>
                          Complete Work Order
                        </AlertDialogAction>
                      </AlertDialogFooter>
                    </AlertDialogContent>
                  </AlertDialog>
                </>
              )}
              
              {form.status === WorkOrderStatus.Awaiting && (
                <Button onClick={resumeWorkOrder} className="bg-amber-600 hover:bg-amber-700">
                  <Play className="h-4 w-4 mr-2" />
                  Resume
                </Button>
              )}
            </>
          )}

          <Button variant="outline" onClick={saveWorkOrder}>
            <Save className="h-4 w-4 mr-2" />
            {mode === "create" ? "Create Work Order" : "Save Changes"}
          </Button>
        </div>
      </div>

      {/* Progress Overview - Process Mode Only */}
      {mode === "process" && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Activity className="h-5 w-5" />
              Progress Overview
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <div className="flex justify-between text-sm">
                <span>Overall Progress</span>
                <span>{Math.round(currentProgress)}%</span>
              </div>
              <Progress value={currentProgress} className="h-3" />
            </div>
            
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <div className="text-center p-3 bg-gray-50 rounded-lg">
                <div className="text-2xl font-bold text-teal-600">{formatTime(elapsedTime)}</div>
                <div className="text-sm text-muted-foreground">Elapsed Time</div>
              </div>
              <div className="text-center p-3 bg-gray-50 rounded-lg">
                <div className="text-2xl font-bold">{form.actualLbs || 0}</div>
                <div className="text-sm text-muted-foreground">Processed (lbs)</div>
              </div>
              <div className="text-center p-3 bg-gray-50 rounded-lg">
                <div className="text-2xl font-bold">
                  {elapsedTime > 0 ? Math.round((form.actualLbs || 0) / (elapsedTime / 3600)) : 0}
                </div>
                <div className="text-sm text-muted-foreground">Rate (lbs/hr)</div>
              </div>
              <div className="text-center p-3 bg-gray-50 rounded-lg">
                <div className="text-2xl font-bold">
                  {form.scheduledEndDate ? new Date(form.scheduledEndDate).toLocaleTimeString().slice(0, 5) : "--:--"}
                </div>
                <div className="text-sm text-muted-foreground">Est. Complete</div>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-6">
        {/* Main Content */}
        <div className="xl:col-span-2 space-y-6">
          
          {/* Simplified Setup */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Settings className="h-5 w-5" />
                Machine Setup
              </CardTitle>
              <CardDescription>Select production machine and enter coil tag number for automatic order matching</CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              
              {/* Machine Selection */}
              <div className="space-y-2">
                <Label>Production Machine *</Label>
                <Select 
                  value={form.machineId?.toString() || ""} 
                  onValueChange={(value) => {
                    const machine = machines.find(m => m.id === parseInt(value));
                    if (machine) selectMachine(machine);
                  }}
                  disabled={mode === "process"}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select a production machine" />
                  </SelectTrigger>
                  <SelectContent>
                    {machines.filter(m => m.isActive).map((machine) => (
                      <SelectItem key={machine.id} value={machine.id.toString()}>
                        <div className="flex items-center justify-between w-full">
                          <div className="flex items-center gap-2">
                            <span>{machine.name}</span>
                            <Badge variant="outline" className="text-xs">{machine.category}</Badge>
                          </div>
                          <div className="flex items-center gap-1 ml-4">
                            <Badge className={`text-xs ${machineStatusColors[machine.currentStatus]}`}>
                              {machine.currentStatus}
                            </Badge>
                          </div>
                        </div>
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                
                {form.machine && (
                  <div className="p-3 bg-teal-50 rounded-lg text-sm">
                    <div className="flex items-center justify-between">
                      <span className="text-muted-foreground">Capacity:</span>
                      <span className="font-medium">{form.machine.estimatedLbsPerHour.toLocaleString()} lbs/hr</span>
                    </div>
                    <div className="flex items-center justify-between">
                      <span className="text-muted-foreground">Max Skid:</span>
                      <span className="font-medium">{form.machine.maxSkidCapacity.toLocaleString()} lbs</span>
                    </div>
                  </div>
                )}
              </div>

              <Separator />

              {/* Tag Number Input */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="tagNumber">Coil Tag Number *</Label>
                  <Input
                    id="tagNumber"
                    placeholder="Enter coil tag number (e.g., HRC-24-48-001)"
                    value={form.tagNumber}
                    onChange={(e) => updateForm("tagNumber", e.target.value)}
                    disabled={mode === "process"}
                  />
                  {form.masterCoil && (
                    <div className="p-3 bg-blue-50 rounded-lg text-sm space-y-1">
                      <div className="flex items-center justify-between">
                        <span className="text-muted-foreground">Material:</span>
                        <span className="font-medium">{form.masterCoil.material}</span>
                      </div>
                      <div className="flex items-center justify-between">
                        <span className="text-muted-foreground">Gauge/Width:</span>
                        <span className="font-medium">{form.masterCoil.gauge} × {form.masterCoil.width}"</span>
                      </div>
                      <div className="flex items-center justify-between">
                        <span className="text-muted-foreground">Remaining:</span>
                        <span className="font-medium">{form.masterCoil.remainingWeight.toLocaleString()} lbs</span>
                      </div>
                      <div className="flex items-center justify-between">
                        <span className="text-muted-foreground">Location:</span>
                        <span className="font-medium">{form.masterCoil.location}</span>
                      </div>
                    </div>
                  )}
                </div>
                
                <div className="space-y-4">
                  <div className="space-y-2">
                    <Label htmlFor="priority">Priority</Label>
                    <Select value={form.priority} onValueChange={(value) => updateForm("priority", value)}>
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="low">Low</SelectItem>
                        <SelectItem value="normal">Normal</SelectItem>
                        <SelectItem value="high">High</SelectItem>
                        <SelectItem value="urgent">Urgent</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="dueDate">Production Date *</Label>
                    <Input
                      id="dueDate"
                      type="date"
                      value={form.dueDate}
                      onChange={(e) => updateForm("dueDate", e.target.value)}
                      disabled={mode === "process"}
                    />
                  </div>
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="instructions">Special Instructions</Label>
                <Textarea
                  id="instructions"
                  placeholder="Enter any special instructions for operators..."
                  value={form.instructions || ""}
                  onChange={(e) => updateForm("instructions", e.target.value)}
                  rows={3}
                />
              </div>
            </CardContent>
          </Card>

          {/* Matched Orders */}
          {form.matchedOrders.length > 0 && (
            <Card>
              <CardHeader>
                <div className="flex items-center justify-between">
                  <div>
                    <CardTitle className="flex items-center gap-2">
                      <Target className="h-5 w-5" />
                      Matched Orders ({form.matchedOrders.length})
                    </CardTitle>
                    <CardDescription>
                      Orders automatically matched based on coil capabilities
                    </CardDescription>
                  </div>
                  {form.isMultiWorkOrder && (
                    <Button 
                      variant="outline" 
                      size="sm"
                      onClick={() => setShowSplitDetails(!showSplitDetails)}
                    >
                      <Split className="h-4 w-4 mr-1" />
                      {showSplitDetails ? 'Hide' : 'Show'} Split Details
                    </Button>
                  )}
                </div>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {form.matchedOrders.map(order => (
                    <Card key={order.id} className="bg-green-50 border-green-200">
                      <CardContent className="p-4">
                        <div className="flex items-center justify-between mb-3">
                          <div>
                            <h4 className="font-medium">{order.customerName}</h4>
                            <p className="text-sm text-muted-foreground">{order.orderNumber}</p>
                          </div>
                          <div className="flex items-center gap-2">
                            <Badge className={priorityColors[order.priority]}>{order.priority}</Badge>
                            <Badge variant="outline">Due: {new Date(order.dueDate).toLocaleDateString()}</Badge>
                          </div>
                        </div>
                        
                        <div className="space-y-2">
                          {order.lineItems.map(lineItem => {
                            const workOrderItems = form.workOrderLineItems.filter(woi => woi.orderLineItemId === lineItem.id);
                            const customer = customers.find(c => c.id === order.customerId);
                            
                            return (
                              <div key={lineItem.id} className="p-3 bg-white rounded border">
                                <div className="flex items-center justify-between mb-2">
                                  <div>
                                    <p className="font-medium text-sm">{lineItem.description}</p>
                                    <p className="text-xs text-muted-foreground">
                                      {lineItem.orderQuantity} pcs • {lineItem.orderWeight.toLocaleString()} lbs
                                    </p>
                                    {customer && (
                                      <div className="flex items-center gap-2 mt-1">
                                        <Badge variant="outline" className="text-xs bg-blue-50">
                                          Max Skid: {customer.maxSkidCapacity.toLocaleString()} lbs
                                        </Badge>
                                        {customer.specialInstructions && (
                                          <Badge variant="outline" className="text-xs bg-yellow-50">
                                            Special Instructions
                                          </Badge>
                                        )}
                                      </div>
                                    )}
                                  </div>
                                  {workOrderItems.length > 1 && (
                                    <Badge variant="outline" className="bg-orange-100 text-orange-700">
                                      Split into {workOrderItems.length} WOs
                                    </Badge>
                                  )}
                                </div>
                                
                                {showSplitDetails && workOrderItems.length > 0 && (
                                  <div className="mt-2 space-y-2">
                                    {workOrderItems.map((woi, index) => (
                                      <div key={woi.id} className="p-3 bg-gray-50 rounded space-y-2">
                                        <div className="flex items-center justify-between">
                                          <span className="text-sm font-medium">WO {index + 1}</span>
                                          <div className="flex items-center gap-2">
                                            {woi.manuallyAdjusted && (
                                              <Badge variant="outline" className="text-xs bg-green-100 text-green-700">
                                                Manual
                                              </Badge>
                                            )}
                                            {woi.splitReason && (
                                              <Badge variant="outline" className="text-xs">
                                                {woi.splitReason === 'skid-capacity' ? 'Skid limit' : 
                                                 woi.splitReason === 'coil-capacity' ? 'Coil limit' : 'Split'}
                                              </Badge>
                                            )}
                                            <Button
                                              variant="outline"
                                              size="sm"
                                              onClick={() => removeLineItem(woi.id)}
                                              className="h-6 w-6 p-0 text-red-600 hover:bg-red-50"
                                            >
                                              ×
                                            </Button>
                                          </div>
                                        </div>
                                        
                                        <div className="grid grid-cols-2 gap-2">
                                          <div className="space-y-1">
                                            <Label className="text-xs">Quantity (pcs)</Label>
                                            <Input
                                              type="number"
                                              value={woi.plannedQuantity}
                                              onChange={(e) => updateLineItemQuantity(woi.id, parseInt(e.target.value) || 0)}
                                              className="h-8 text-sm"
                                              min="0"
                                              max={lineItem.remainingQuantity}
                                            />
                                          </div>
                                          <div className="space-y-1">
                                            <Label className="text-xs">Weight (lbs)</Label>
                                            <Input
                                              type="number"
                                              value={Math.round(woi.plannedWeight)}
                                              onChange={(e) => updateLineItemWeight(woi.id, parseInt(e.target.value) || 0)}
                                              className="h-8 text-sm"
                                              min="0"
                                              max={lineItem.remainingWeight}
                                            />
                                          </div>
                                        </div>
                                        
                                        {customer?.specialInstructions && (
                                          <div className="p-2 bg-yellow-50 rounded text-xs">
                                            <span className="font-medium text-yellow-800">Customer Note: </span>
                                            <span className="text-yellow-700">{customer.specialInstructions}</span>
                                          </div>
                                        )}
                                      </div>
                                    ))}
                                  </div>
                                )}
                              </div>
                            );
                          })}
                        </div>
                      </CardContent>
                    </Card>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          
          {/* Summary */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Package className="h-5 w-5" />
                Work Order Summary
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-3">
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">Total Orders:</span>
                  <span className="font-medium">{form.matchedOrders.length}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">Line Items:</span>
                  <span className="font-medium">{form.workOrderLineItems.length}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">Total Weight:</span>
                  <span className="font-medium">{getTotalWeight().toLocaleString()} lbs</span>
                </div>
                {form.machine && (
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">Est. Duration:</span>
                    <span className="font-medium">
                      {Math.round(getTotalWeight() / form.machine.estimatedLbsPerHour * 60)} min
                    </span>
                  </div>
                )}
              </div>

              {form.isMultiWorkOrder && (
                <div className="p-3 bg-orange-50 rounded-lg">
                  <div className="flex items-center gap-2 mb-2">
                    <AlertTriangle className="h-4 w-4 text-orange-600" />
                    <span className="text-sm font-medium text-orange-800">Multi Work Order</span>
                  </div>
                  <p className="text-xs text-orange-700">
                    This will create {form.totalWorkOrders} work orders due to capacity constraints.
                  </p>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Customer Information */}
          {form.matchedOrders.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Info className="h-5 w-5" />
                  Customer Details
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-3 text-sm">
                  {Array.from(new Set(form.matchedOrders.map(order => order.customerId))).map(customerId => {
                    const customer = customers.find(c => c.id === customerId);
                    const customerOrders = form.matchedOrders.filter(order => order.customerId === customerId);
                    
                    return customer ? (
                      <div key={customerId} className="p-3 bg-gray-50 rounded">
                        <div className="flex items-center justify-between mb-2">
                          <span className="font-medium">{customer.name}</span>
                          <Badge variant="outline" className="text-xs">
                            {customerOrders.length} order{customerOrders.length !== 1 ? 's' : ''}
                          </Badge>
                        </div>
                        <div className="space-y-1 text-xs text-muted-foreground">
                          <div className="flex justify-between">
                            <span>Max Skid Capacity:</span>
                            <span className="font-medium">{customer.maxSkidCapacity.toLocaleString()} lbs</span>
                          </div>
                          <div className="flex justify-between">
                            <span>Preferred Delivery:</span>
                            <span className="font-medium capitalize">{customer.preferredDeliveryTime}</span>
                          </div>
                          {customer.specialInstructions && (
                            <div className="mt-2 p-2 bg-yellow-50 rounded">
                              <span className="font-medium text-yellow-800">Special Instructions:</span>
                              <p className="text-yellow-700">{customer.specialInstructions}</p>
                            </div>
                          )}
                        </div>
                      </div>
                    ) : null;
                  })}
                </div>
              </CardContent>
            </Card>
          )}

          {/* Split Analysis */}
          {form.workOrderLineItems.some(woi => woi.splitReason) && (
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Split className="h-5 w-5" />
                  Split Analysis
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-3 text-sm">
                  {form.workOrderLineItems.filter(woi => woi.splitReason === 'skid-capacity').length > 0 && (
                    <div className="p-2 bg-yellow-50 rounded">
                      <span className="font-medium text-yellow-800">Customer Skid Limits:</span>
                      <p className="text-yellow-700">
                        {form.workOrderLineItems.filter(woi => woi.splitReason === 'skid-capacity').length} items split due to customer skid capacity constraints
                      </p>
                    </div>
                  )}
                  {form.workOrderLineItems.filter(woi => woi.splitReason === 'coil-capacity').length > 0 && (
                    <div className="p-2 bg-red-50 rounded">
                      <span className="font-medium text-red-800">Coil Capacity:</span>
                      <p className="text-red-700">
                        {form.workOrderLineItems.filter(woi => woi.splitReason === 'coil-capacity').length} items exceed remaining coil weight
                      </p>
                    </div>
                  )}
                  {form.workOrderLineItems.filter(woi => woi.manuallyAdjusted).length > 0 && (
                    <div className="p-2 bg-green-50 rounded">
                      <span className="font-medium text-green-800">Manual Adjustments:</span>
                      <p className="text-green-700">
                        {form.workOrderLineItems.filter(woi => woi.manuallyAdjusted).length} items manually adjusted by user
                      </p>
                    </div>
                  )}
                </div>
              </CardContent>
            </Card>
          )}

        </div>
      </div>
    </div>
  );
}