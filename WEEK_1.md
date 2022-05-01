# Requirements breakdown
## Authentication (`Auths`)
Managed service. 

Emits business events (BEs):
- `Auths.AccountCreated`
- `Auths.AccountRoleChanged`
  
Emits data events (DEs):
- `Auths.AccountCreated = { uid: Guid; role: Role; }`
- `Auths.AccountModified = { uid: Guid; new_role: Role }`
## Task tracker (`Tasks`)
### Task tracker should be available to all users (query)
- Actor: `Tasks.Account`
- Command: `Tasks.Cmd.GetAllTasks`

### Authorization should be done from the managed service (data duplication in service)
- Actor: DEs `Auths.AccountCreated` or `Auths.AccountModified`

### Anyone can create tasks
- Actor: `Tasks.Account`
- Command: `Tasks.Cmd.CreateTask = { description: string; assigned_to: Tasks.Account.uid }`
- Event: `Tasks.TaskCreated = { description: string; assigned_to: Tasks.Account; is_done: false; uid: Guid }`

### Managers can shuffle tasks
- Actor: `Tasks.Account` where `role = Manager | Admin`
- Command: `Tasks.Cmd.ShuffleAllTasks`
- Data: `Tasks.Task[]`
- Event: `(Tasks.TaskReassigned = { user_uid_was: Guid; user_uid_now: Guid; task_uid: Guid})[]`

### Anyone can complete their task
- Actor: `Tasks.Account`
- Command: `Tasks.Cmd.CompleteTask`
- Event: `Tasks.TaskCompleted`

## Accounting (`Accountings`)
### Authorization should be done from the managed service (data duplication in service)
- Actor: events `Auths.AccountCreated` or `Auths.AccountModified`

### When task is completed user balance should be increased
- Actor: event `Tasks.TaskCompleted`
- Command: `Accountings.Cmd.AddToBalanceDueToTaskCompletion`
- Event: `Accountings.UserBalanceIncreased = { user_uid: Guid; balance_increase: uint }`

### When task is reassigned user balance should be decreased
- Actor: event `Tasks.TaskReassigned`
- Command: `Accountings.Cmd.SubFromBalanceDueToTaskReassignment`
- Event: `Accountings.UserBalanceDecreased = { user_uid: Guid; balance_decreased: uint }`

### At the end of the day all of the accounts should be reconciled
- Actor: CRON
- Command: `Accountings.Cmd.Reconcile`
- Event: `Accountings.UserBalanceReconciled[]` `Accounting.UserPaymentRegistered[]`

### When payment is processed, it's state needs to be updated
- Actor: event `Payments.PaymentProcessed`
- Command: `Accountings.Cmd.UpdatePaymentStatus`
- Event: `Accountings.UpdatedPaymentStatus` (won't be used?)

## Payments (`Payments`)
### Payments should be processed
- Actor: event `Accountings.PaymentRegistered`
- Command: `Payments.Cmd.ProcessPayment`
- Event: `Payments.PaymentProcessed`

## Analytics (`Analytics`)
A service that replicates data from `Auths` (only to authorize users) and `Accountings` to provide efficient read views.

# Diagram
![diagram](/img/Screenshot%202022-05-01%20at%2019.57.30.png)